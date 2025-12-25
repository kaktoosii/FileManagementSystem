using Base.DomainClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ViewModels.Dto;

namespace Services.Contracts;

public interface IDocumentService
{
    Task<Document> GetDocumentById(Guid documentId);
    Task<Guid> UploadDocument(DocumentDto document, int userId, string userIp, string filePath = "");
    Task<FileStream> DownloadDocument(Guid documentId);
    Task<bool> DeleteImageAsync(Guid image);
    Task<bool> DeleteDocumentOnlyImageFileAsync(Guid documentId);
    FileStream BrokenImage();
    string SaveExcelDocument(string uniqeFilename, IFormFile document);
}