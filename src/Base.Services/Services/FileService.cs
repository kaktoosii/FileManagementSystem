using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using ViewModels.Dto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IUnitOfWork _uow;
    private readonly DbSet<Base.DomainClasses.File> _files;
    private readonly IFilePatternService _filePatternService;
    private readonly IFolderService _folderService;

    public FileService(
        IUnitOfWork uow,
        IWebHostEnvironment hostingEnvironment,
        IFilePatternService filePatternService,
        IFolderService folderService)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _filePatternService = filePatternService ?? throw new ArgumentNullException(nameof(filePatternService));
        _folderService = folderService ?? throw new ArgumentNullException(nameof(folderService));
        _files = _uow.Set<Base.DomainClasses.File>();
    }

    public async Task<int> UploadFileAsync(UploadFileDto fileDto, int userId, string userIp)
    {
        if (fileDto?.File == null)
            throw new AppException("فایل نمی‌تواند خالی باشد");


        // Validate file size and extension
        if (!IsValidFileSize(fileDto.File.Length, fileDto.File.ContentType))
            throw new AppException("حجم فایل غیر مجاز است");

        if (!IsValidExtension(Path.GetExtension(fileDto.File.FileName), fileDto.File.ContentType))
            throw new AppException("نوع فایل غیر مجاز است");

        // Validate folder if provided
        if (fileDto.FolderId.HasValue)
        {
            var folder = await _uow.Set<Folder>()
                .FirstOrDefaultAsync(f => f.Id == fileDto.FolderId.Value && f.UserId == userId && !f.IsDeleted);
            
            if (folder == null)
                throw new AppException("فولدر یافت نشد یا دسترسی ندارید");
        }

        string originalFileName = fileDto.File.FileName;
        string generatedFileName;
        string patternValuesJson = null;
        var date = DateTime.Now.ToEpochMilliseconds().ToString(CultureInfo.CurrentCulture);
        // Generate filename from pattern if provided
        if (fileDto.FilePatternId.HasValue && fileDto.PatternValues != null && fileDto.PatternValues.Count != 0)
        {
            try
            {
                var generatedName = await _filePatternService.GenerateFileNameAsync(
                    fileDto.FilePatternId.Value,
                    fileDto.PatternValues);

                var extension = Path.GetExtension(originalFileName);
                
                generatedFileName = $"{date}_{generatedName}{extension}";
                patternValuesJson = JsonSerializer.Serialize(fileDto.PatternValues);
            }
            catch (Exception ex)
            {
                throw new AppException($"خطا در تولید نام فایل از پترن: {ex.Message}");
            }
        }
        else
        {
            var extension = Path.GetExtension(originalFileName);
            generatedFileName = $"{date}_{Path.GetFileNameWithoutExtension(originalFileName)}{extension}";
        }

        // Determine save path
        string savePath = @"\UploadedFiles\";
        if (fileDto.FolderId.HasValue)
        {
            try
            {
                var folderPath = await _folderService.GetFolderPathAsync(fileDto.FolderId.Value);
                savePath = @"\UploadedFiles\" + folderPath.Replace("\\", "\\", StringComparison.OrdinalIgnoreCase) + "\\";
            }
            catch (Exception ex)
            {
                throw new AppException($"خطا در دریافت مسیر فولدر: {ex.Message}");
            }
        }

        // Save file to disk
        string savedPath = await SaveFileAsync(generatedFileName, fileDto.File, savePath);

        // Create file entity
        var fileEntity = new Base.DomainClasses.File(
            savedPath,
            generatedFileName,
            userId,
            userIp,
            fileDto.File.ContentType,
            fileDto.File.Length,
            fileDto.FolderId,
            fileDto.FilePatternId,
            patternValuesJson
        );

        fileEntity.UpdateOriginalFileName(originalFileName);

        await _files.AddAsync(fileEntity);
        await _uow.SaveChangesAsync();

        return fileEntity.Id;
    }

    public async Task<FileDto> GetFileByIdAsync(int fileId)
    {
        var file = await _files
            .Include(f => f.Folder)
            .Include(f => f.FilePattern)
            .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

        if (file == null)
            return null;

        return new FileDto
        {
            Id = file.Id,
            Path = file.Path,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            UserId = file.UserId,
            RegisterDate = file.RegisterDate,
            MimeType = file.MimeType,
            FileSize = file.FileSize,
            FolderId = file.FolderId,
            FolderName = file.Folder?.Name,
            FilePatternId = file.FilePatternId,
            FilePatternName = file.FilePattern?.Name,
            PatternValues = file.PatternValues
        };
    }

    public async Task<List<FileDto>> GetFilesByFolderIdAsync(int folderId)
    {
        var files = await _files
            .Include(f => f.Folder)
            .Include(f => f.FilePattern)
            .Where(f => f.FolderId == folderId && !f.IsDeleted)
            .OrderByDescending(f => f.RegisterDate)
            .ToListAsync();

        return files.Select(f => new FileDto
        {
            Id = f.Id,
            Path = f.Path,
            FileName = f.FileName,
            OriginalFileName = f.OriginalFileName,
            UserId = f.UserId,
            RegisterDate = f.RegisterDate,
            MimeType = f.MimeType,
            FileSize = f.FileSize,
            FolderId = f.FolderId,
            FolderName = f.Folder?.Name,
            FilePatternId = f.FilePatternId,
            FilePatternName = f.FilePattern?.Name,
            PatternValues = f.PatternValues
        }).ToList();
    }

    public async Task<List<FileDto>> GetFilesByUserIdAsync(int userId)
    {
        var files = await _files
            .Include(f => f.Folder)
            .Include(f => f.FilePattern)
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .OrderByDescending(f => f.RegisterDate)
            .ToListAsync();

        return files.Select(f => new FileDto
        {
            Id = f.Id,
            Path = f.Path,
            FileName = f.FileName,
            OriginalFileName = f.OriginalFileName,
            UserId = f.UserId,
            RegisterDate = f.RegisterDate,
            MimeType = f.MimeType,
            FileSize = f.FileSize,
            FolderId = f.FolderId,
            FolderName = f.Folder?.Name,
            FilePatternId = f.FilePatternId,
            FilePatternName = f.FilePattern?.Name,
            PatternValues = f.PatternValues
        }).ToList();
    }

    public async Task<FileStream> DownloadFileAsync(int fileId)
    {
        var file = await _files.FindAsync(fileId);

        if (file == null || file.IsDeleted)
            throw new FileNotFoundException("فایل یافت نشد");

        var rootPath = _hostingEnvironment.ContentRootPath;
        // Path is stored with forward slashes, convert to backslashes for Windows
        var normalizedPath = file.Path.Replace('/', '\\').TrimStart('\\', '/');
        var fullPath = Path.Combine(rootPath, normalizedPath);

        if (!System.IO.File.Exists(fullPath))
            throw new FileNotFoundException($"فایل فیزیکی یافت نشد: {fullPath}");

        return System.IO.File.OpenRead(fullPath);
    }

    public async Task<bool> DeleteFileAsync(int fileId, int userId)
    {
        var file = await _files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId && !f.IsDeleted);

        if (file == null)
            throw new AppException("فایل یافت نشد یا دسترسی ندارید");

        file.SoftDelete();
        await _uow.SaveChangesAsync();

        // Optionally delete physical file
        try
        {
            var rootPath = _hostingEnvironment.ContentRootPath;
            var fullPath = Path.Combine(rootPath, file.Path.TrimStart('\\'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch
        {
            // Log error but don't fail the operation
        }

        return true;
    }

    public async Task<bool> MoveFileAsync(int fileId, int? newFolderId, int userId)
    {
        var file = await _files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId && !f.IsDeleted);

        if (file == null)
            throw new AppException("فایل یافت نشد یا دسترسی ندارید");

        if (newFolderId.HasValue)
        {
            var folder = await _uow.Set<Folder>()
                .FirstOrDefaultAsync(f => f.Id == newFolderId.Value && f.UserId == userId && !f.IsDeleted);

            if (folder == null)
                throw new AppException("فولدر یافت نشد یا دسترسی ندارید");
        }

        file.UpdateFolder(newFolderId);
        await _uow.SaveChangesAsync();

        return true;
    }

    private async Task<string> SaveFileAsync(string fileName, IFormFile file, string relativePath)
    {
        var rootPath = _hostingEnvironment.ContentRootPath;
        var fullPath = Path.Combine(rootPath, relativePath.TrimStart('\\'), fileName);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var stream = new System.IO.FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine(relativePath, fileName).Replace('\\', '/');
    }

    private static bool IsValidFileSize(long fileSize, string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        CultureInfo cultureInfo = new CultureInfo("en-US", false);
        if (contentType.ToLower(cultureInfo).Contains("image", StringComparison.Ordinal))
        {
            return fileSize <= 10 * 1024 * 1024; // 10 MB
        }
        else
        {
            return fileSize <= 500 * 1024 * 1024; // 500 MB
        }
    }

    private static bool IsValidExtension(string extension, string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        CultureInfo cultureInfo = new CultureInfo("en-US", false);
        string[] validExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };

        if (contentType.ToLower(cultureInfo).Contains("video", StringComparison.Ordinal))
        {
            validExtensions = new[] { ".mp4", ".3gp", ".wmv", ".flv", ".avi", ".mpg", ".mpeg" };
        }

        return validExtensions.Any(x => x.ToLower(cultureInfo).Equals(extension, StringComparison.Ordinal));
    }
}
