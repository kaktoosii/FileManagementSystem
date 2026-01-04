using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Services.Contracts;
using ViewModels.Dto;
using System.Linq;
using System.IO;
using System;

namespace Services.Services;

public class FolderService : IFolderService
{
    private readonly IUnitOfWork _uow;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly DbSet<Folder> _folders;
    private readonly DbSet<Base.DomainClasses.File> _files;

    public FolderService(IUnitOfWork uow, IWebHostEnvironment hostingEnvironment)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _folders = _uow.Set<Folder>();
        _files = _uow.Set<Base.DomainClasses.File>();
    }

    public async Task<int> CreateFolderAsync(CreateFolderDto folderDto, int userId)
    {
        if (folderDto == null)
            throw new AppException("فولدر نمی‌تواند خالی باشد");

        if (string.IsNullOrWhiteSpace(folderDto.Name))
            throw new AppException("نام فولدر نمی‌تواند خالی باشد");

        // Check if parent folder exists and belongs to user
        if (folderDto.ParentFolderId.HasValue)
        {
            var parentFolder = await _folders
                .FirstOrDefaultAsync(f => f.Id == folderDto.ParentFolderId.Value && f.UserId == userId && !f.IsDeleted);

            if (parentFolder == null)
                throw new AppException("فولدر والد یافت نشد");
        }

        // Check for duplicate name in same parent
        var exists = await _folders.AnyAsync(f =>
            f.Name == folderDto.Name &&
            f.ParentFolderId == folderDto.ParentFolderId &&
            f.UserId == userId &&
            !f.IsDeleted);

        if (exists)
            throw new AppException("فولدری با این نام در همین مکان وجود دارد");

        var folder = new Folder(folderDto.Name, folderDto.ParentFolderId, userId);
        if (!string.IsNullOrWhiteSpace(folderDto.Description))
            folder.UpdateDescription(folderDto.Description);

        await _folders.AddAsync(folder);
        await _uow.SaveChangesAsync();
        return folder.Id;
    }

    public async Task<FolderDto> GetFolderByIdAsync(int folderId)
    {
        var folder = await _folders
            .Include(f => f.ParentFolder)
            .FirstOrDefaultAsync(f => f.Id == folderId && !f.IsDeleted);

        if (folder == null)
            return null;

        var documentCount = await _files.CountAsync(d => d.FolderId == folderId && !d.IsDeleted);

        return new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            Description = folder.Description,
            ParentFolderId = folder.ParentFolderId,
            ParentFolderName = folder.ParentFolder?.Name,
            UserId = folder.UserId,
            CreateDate = folder.CreateDate,
            DocumentCount = documentCount
        };
    }

    public async Task<List<FolderDto>> GetFoldersByUserIdAsync(int userId)
    {
        var folders = await _folders
            .Include(f => f.ParentFolder)
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .OrderBy(f => f.Name)
            .ToListAsync();

        var result = new List<FolderDto>();
        foreach (var folder in folders)
        {
            var documentCount = await _files.CountAsync(d => d.FolderId == folder.Id);
            result.Add(new FolderDto
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                ParentFolderId = folder.ParentFolderId,
                ParentFolderName = folder.ParentFolder?.Name,
                UserId = folder.UserId,
                CreateDate = folder.CreateDate,
                DocumentCount = documentCount
            });
        }

        return result;
    }

    public async Task<List<FolderDto>> GetRootFoldersByUserIdAsync(int userId)
    {
        var folders = await _folders
            .Include(f => f.ParentFolder)
            .Where(f => f.UserId == userId && f.ParentFolderId == null && !f.IsDeleted)
            .OrderBy(f => f.Name)
            .ToListAsync();

        var result = new List<FolderDto>();
        foreach (var folder in folders)
        {
            var documentCount = await _files.CountAsync(d => d.FolderId == folder.Id);
            //var subFolderCount = await _folders.CountAsync(f => f.ParentFolderId == folder.Id && !f.IsDeleted);

            result.Add(new FolderDto
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                ParentFolderId = null,
                UserId = folder.UserId,
                CreateDate = folder.CreateDate,
                DocumentCount = documentCount
            });
        }

        return result;
    }

    public async Task<List<FolderDto>> GetSubFoldersAsync(int parentFolderId)
    {
        var folders = await _folders
            .Include(f => f.ParentFolder)
            .Where(f => f.ParentFolderId == parentFolderId && !f.IsDeleted)
            .OrderBy(f => f.Name)
            .ToListAsync();

        var result = new List<FolderDto>();
        foreach (var folder in folders)
        {
            var documentCount = await _files.CountAsync(d => d.FolderId == folder.Id);
            result.Add(new FolderDto
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                ParentFolderId = folder.ParentFolderId,
                ParentFolderName = folder.ParentFolder?.Name,
                UserId = folder.UserId,
                CreateDate = folder.CreateDate,
                DocumentCount = documentCount
            });
        }

        return result;
    }

    public async Task UpdateFolderAsync(int folderId, UpdateFolderDto folderDto, int userId)
    {
        if (folderDto == null)
            throw new AppException("فولدر نمی‌تواند خالی باشد");

        var folder = await _folders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId && !f.IsDeleted);

        if (folder == null)
            throw new AppException("فولدر یافت نشد");

        // Check if parent folder exists and belongs to user
        if (folderDto.ParentFolderId.HasValue && folderDto.ParentFolderId.Value != folder.ParentFolderId)
        {
            var parentFolder = await _folders
                .FirstOrDefaultAsync(f => f.Id == folderDto.ParentFolderId.Value && f.UserId == userId && !f.IsDeleted);

            if (parentFolder == null)
                throw new AppException("فولدر والد یافت نشد");

            // Prevent circular reference
            if (await IsDescendantOf(folderDto.ParentFolderId.Value, folderId))
                throw new AppException("نمی‌توان فولدر را به زیرشاخه خودش منتقل کرد");
        }

        // Check for duplicate name
        if (!string.Equals(folderDto.Name, folder.Name, StringComparison.Ordinal))
        {
            var exists = await _folders.AnyAsync(f =>
                f.Name == folderDto.Name &&
                f.ParentFolderId == folderDto.ParentFolderId &&
                f.UserId == userId &&
                f.Id != folderId &&
                !f.IsDeleted);

            if (exists)
                throw new AppException("فولدری با این نام در همین مکان وجود دارد");
        }

        var oldName = folder.Name;
        var nameChanged = !string.Equals(folderDto.Name, folder.Name, StringComparison.Ordinal);

        folder.UpdateName(folderDto.Name);
        folder.UpdateDescription(folderDto.Description);
        if (folderDto.ParentFolderId != folder.ParentFolderId)
            folder.MoveToFolder(folderDto.ParentFolderId);

        await _uow.SaveChangesAsync();

        // Rename physical folder and update paths of files and subfolders if folder name changed
        if (nameChanged)
        {
            await RenamePhysicalFolderAsync(folderId, oldName, folderDto.Name);
            await UpdateFolderPathsRecursivelyAsync(folderId);
        }
    }

    public async Task DeleteFolderAsync(int folderId, int userId)
    {
        var folder = await _folders
            .Include(f => f.SubFolders)
            .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId && !f.IsDeleted);

        if (folder == null)
            throw new AppException("فولدر یافت نشد");

        // Check if folder has subfolders
        var hasSubFolders = await _folders.AnyAsync(f => f.ParentFolderId == folderId && !f.IsDeleted);
        if (hasSubFolders)
            throw new AppException("نمی‌توان فولدری که دارای زیرشاخه است را حذف کرد");

        // Check if folder has files
        var hasFiles = await _files.AnyAsync(d => d.FolderId == folderId && !d.IsDeleted);
        if (hasFiles)
            throw new AppException("نمی‌توان فولدری که دارای فایل است را حذف کرد");

        folder.SoftDelete();
        await _uow.SaveChangesAsync();
    }

    public async Task MoveFolderAsync(int folderId, int? newParentFolderId, int userId)
    {
        var folder = await _folders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId && !f.IsDeleted);

        if (folder == null)
            throw new AppException("فولدر یافت نشد");

        if (newParentFolderId.HasValue)
        {
            var parentFolder = await _folders
                .FirstOrDefaultAsync(f => f.Id == newParentFolderId.Value && f.UserId == userId && !f.IsDeleted);

            if (parentFolder == null)
                throw new AppException("فولدر والد یافت نشد");

            // Prevent circular reference
            if (await IsDescendantOf(newParentFolderId.Value, folderId))
                throw new AppException("نمی‌توان فولدر را به زیرشاخه خودش منتقل کرد");
        }

        folder.MoveToFolder(newParentFolderId);
        await _uow.SaveChangesAsync();
    }

    public async Task<string> GetFolderPathAsync(int folderId)
    {
        var pathParts = new List<string>();
        var currentFolderId = (int?)folderId;

        while (currentFolderId.HasValue)
        {
            var folder = await _folders
                .FirstOrDefaultAsync(f => f.Id == currentFolderId.Value && !f.IsDeleted);

            if (folder == null)
                break;

            pathParts.Insert(0, folder.Name);
            currentFolderId = folder.ParentFolderId;
        }

        return string.Join("\\", pathParts);
    }

    private async Task<bool> IsDescendantOf(int ancestorId, int descendantId)
    {
        var currentFolderId = (int?)descendantId;
        var visited = new HashSet<int>();

        while (currentFolderId.HasValue && !visited.Contains(currentFolderId.Value))
        {
            if (currentFolderId.Value == ancestorId)
                return true;

            visited.Add(currentFolderId.Value);
            var folder = await _folders
                .FirstOrDefaultAsync(f => f.Id == currentFolderId.Value && !f.IsDeleted);

            if (folder == null)
                break;

            currentFolderId = folder.ParentFolderId;
        }

        return false;
    }

    private async Task RenamePhysicalFolderAsync(int folderId, string oldName, string newName)
    {
        try
        {
            // Get folder path (this includes parent folders)
            var folderPath = await GetFolderPathAsync(folderId);
            var rootPath = _hostingEnvironment.ContentRootPath;
            var uploadBasePath = Path.Combine(rootPath, "UploadedFiles");
            
            // Build old folder path (with old name)
            var oldFolderPath = Path.Combine(uploadBasePath, folderPath.Replace('\\', Path.DirectorySeparatorChar));
            
            // Build new folder path (replace only the last occurrence of old name with new name)
            var pathParts = folderPath.Split('\\');
            var lastIndex = Array.LastIndexOf(pathParts, oldName);
            if (lastIndex >= 0)
            {
                pathParts[lastIndex] = newName;
            }
            var newFolderPath = Path.Combine(uploadBasePath, string.Join(Path.DirectorySeparatorChar.ToString(), pathParts));

            // Only rename if the folder exists and paths are different
            if (Directory.Exists(oldFolderPath) && !string.Equals(oldFolderPath, newFolderPath, StringComparison.Ordinal) && !Directory.Exists(newFolderPath))
            {
                Directory.Move(oldFolderPath, newFolderPath);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the operation
            // You may want to log this exception
            System.Diagnostics.Debug.WriteLine($"Error renaming physical folder: {ex.Message}");
        }
    }

    private async Task UpdateFolderPathsRecursivelyAsync(int folderId)
    {
        // Get all files in this folder
        var files = await _files
            .Where(f => f.FolderId == folderId && !f.IsDeleted)
            .ToListAsync();

        // Update file paths
        foreach (var file in files)
        {
            var newPath = await BuildFilePathAsync(file.FolderId.Value, file.FileName);
            file.UpdatePath(newPath);
        }

        // Get all subfolders
        var subFolders = await _folders
            .Where(f => f.ParentFolderId == folderId && !f.IsDeleted)
            .ToListAsync();

        // Recursively update subfolders
        foreach (var subFolder in subFolders)
        {
            await UpdateFolderPathsRecursivelyAsync(subFolder.Id);
        }

        await _uow.SaveChangesAsync();
    }

    private async Task<string> BuildFilePathAsync(int folderId, string fileName)
    {
        var folderPath = await GetFolderPathAsync(folderId);
        var relativePath = @"\UploadedFiles\" + folderPath.Replace("\\", "\\", StringComparison.OrdinalIgnoreCase) + "\\";
        var fullPath = Path.Combine(relativePath, fileName);
        // Convert to forward slashes and ensure it starts with /
        var normalizedPath = fullPath.Replace('\\', '/');
        return normalizedPath.StartsWith('/') ? normalizedPath : "/" + normalizedPath.TrimStart('/');
    }
}
