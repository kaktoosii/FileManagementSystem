using Base.DataLayer.Context;
using Base.DomainClasses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ViewModels.Dto;
using System.Globalization;
using Base.Common.Helpers;


namespace Services.Services;

public class DocumentService : IDocumentService
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IUnitOfWork _uow;
    private readonly DbSet<Document> _document;

    public DocumentService(
        IUnitOfWork uow,
        IWebHostEnvironment hostingEnvironment)
    {

        this._hostingEnvironment = (hostingEnvironment == null) ?
            throw new ArgumentNullException(nameof(hostingEnvironment))
            : hostingEnvironment;

        _uow = uow;
        _document = _uow.Set<Document>();
    }

    public async Task<Document> GetDocumentById(Guid documentId)
    {
        return await _document.FindAsync(documentId);
    }

    public async Task<Guid> UploadDocument(DocumentDto document, int userId, string userIp, string filePath = "")
    {
        CultureInfo cultureInfo = new CultureInfo("en-US", false);
        if (document == null)
        {
            throw new AppException(nameof(document));
        }
        if (!HasValidSize(document.Image.Length, document.Image.ContentType))
        {
            throw new AppException("تصویر غیر مجاز است");
        }


        if (!HasVaildExtension(Path.GetExtension(document.Image.FileName), document.Image.ContentType))
        {
            throw new AppException("نوع فایل غیر مجاز است");
        }

        Guid documentId = GenerateDocumentId();

        string uniqeFilename = CreateUniqueFileName(document.Image.FileName, documentId);

        string savedPath = await SaveDocument(uniqeFilename, document.Image, filePath);
        var documentModel = new Document(
            savedPath,
            userId,
            userIp,
            Path.GetExtension(document.Image.FileName).ToLower(cultureInfo)
            );
        _ = await this._document.AddAsync(documentModel);
        await this._uow.SaveChangesAsync();
        return documentModel.Id;

    }

    public static bool HasValidSize(long documentFileSize, string contentType)
    {
        if (contentType == null)
        {
            throw new AppException(nameof(contentType));
        }
        CultureInfo cultureInfo = new CultureInfo("en-US", false);
        if (contentType.ToLower(cultureInfo).Contains("image", StringComparison.Ordinal))
        {
            return (documentFileSize <= 5 * 1024 * 1024);
        }
        else
        {
            return (documentFileSize <= 500 * 1024 * 1024);
        }
    }

    public static bool HasVaildExtension(string documentFileExtension, string contentType)
    {
        if (contentType == null)
        {
            throw new AppException(nameof(contentType));
        }
        CultureInfo cultureInfo = new CultureInfo("en-US", false);
        string[] validExtensions = new[] { ".png", ".jpg", ".jpeg" };

        if (contentType.ToLower(cultureInfo).Contains("officedocument", StringComparison.Ordinal) || contentType.ToLower(cultureInfo).Contains("application/msword", StringComparison.Ordinal))
        {
            validExtensions = new[] { ".doc", ".docx" };
        }
        else if (!contentType.ToLower(cultureInfo).Contains("image", StringComparison.Ordinal))
        {
            validExtensions = new[] { ".mp4", ".3gp", ".wmv", ".flv", ".avi", ".mpg", ".mpeg" };
        }
        if (validExtensions.Any(x => x.ToLower(cultureInfo).Equals(documentFileExtension, StringComparison.Ordinal)))
            return true;
        else
            return false;
       

    }
    public static string CreateUniqueFileName(string fileName, Guid documentId)
    {
        FileInfo fileInfo = new FileInfo(fileName);
        return documentId + fileInfo.Extension;
    }

    public async Task<string> SaveDocument(string uniqeFilename, IFormFile document, string filePath = "")
    {
        CultureInfo cultureInfo = new CultureInfo("en-US", false);
        var documentPath = @"\UploadedDocuments\";
        if (document == null)
        {
            throw new AppException(nameof(document));
        }
        if (document.ContentType.ToLower(cultureInfo).Contains("video", StringComparison.Ordinal))
        {
            documentPath = @"\UploadedDocuments\Videos\";
        }

        if (!String.IsNullOrWhiteSpace(filePath))
        {
            documentPath = filePath;
        }

        var rootPath = this._hostingEnvironment.ContentRootPath;
        var relativePath = Path.Combine(documentPath + uniqeFilename);
        var path = Path.Combine(rootPath + relativePath);

        var saveDirectory = Path.Combine(rootPath + documentPath);
        if (!Directory.Exists(saveDirectory))
        {
            System.IO.Directory.CreateDirectory(saveDirectory);
        }

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await document.CopyToAsync(stream);
        }

        return relativePath;
    }
    
    public static Guid GenerateDocumentId()
    {
        return Guid.NewGuid();
    }

    public async Task<FileStream> DownloadDocument(Guid documentId)
    {
        var document = await this._document.FindAsync(documentId);

        if (document == null)
        {
            throw new FileNotFoundException("فایل یافت نشد");
        }

        var rootPath = this._hostingEnvironment.ContentRootPath;
        var path = Path.Combine(rootPath + document.Path);
        if (!File.Exists(path))
        {
            return BrokenImage();
        }
        return File.OpenRead(path);
    }


  

    public async Task<bool> DeleteImageAsync(Guid image)
    {
        var document = await this._document.FindAsync(image);
        var rootPath = this._hostingEnvironment.ContentRootPath;
        var relativePath = Path.Combine(rootPath + document.Path);
        try
        {
            _document.Remove(document);
            await _uow.SaveChangesAsync();
            File.Delete(relativePath);
            return true;
        }
        catch
        {
            throw new FileNotFoundException("فایل یافت نشد");
        }
    }

   


    public async Task<bool> DeleteDocumentOnlyImageFileAsync(Guid documentId)
    {
        var document = await this._document.FindAsync(documentId);
        var rootPath = this._hostingEnvironment.ContentRootPath;
        var relativePath = Path.Combine(rootPath + document.Path);
        try
        {
            File.Delete(relativePath);
            return true;
        }
        catch
        {
            throw new FileNotFoundException("فایل یافت نشد");
        }
    }

    public FileStream BrokenImage()
    {
        var documentPath = this._hostingEnvironment.ContentRootPath + @"\wwwroot\images\brokenImage.png";
        var file = File.OpenRead(documentPath);
        return file;
    }


    public string SaveExcelDocument(string uniqeFilename, IFormFile document)
    {
        if (document == null)
        {
            throw new FileNotFoundException("فایل یافت نشد");
        }
        var documentPath = @"\UploadedDocuments\";
        var rootPath = this._hostingEnvironment.ContentRootPath;
        var relativePath = Path.Combine(documentPath + uniqeFilename);
        var path = Path.Combine(rootPath + relativePath);

        var saveDirectory = Path.Combine(rootPath + documentPath);
        if (!Directory.Exists(saveDirectory))
            System.IO.Directory.CreateDirectory(saveDirectory);

        using (var stream = new FileStream(path, FileMode.Create))
        {
             document.CopyTo(stream);
        }

        return relativePath;
    }
}
