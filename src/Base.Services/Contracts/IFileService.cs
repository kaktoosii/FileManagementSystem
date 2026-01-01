using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using ViewModels.Dto;

namespace Services.Contracts;

public interface IFileService
{
    Task<int> UploadFileAsync(UploadFileDto fileDto, int userId, string userIp);
    Task<FileDto> GetFileByIdAsync(int fileId);
    Task<List<FileDto>> GetFilesByFolderIdAsync(int folderId);
    Task<List<FileDto>> GetFilesByUserIdAsync(int userId);
    Task<System.IO.FileStream> DownloadFileAsync(int fileId);
    Task<bool> DeleteFileAsync(int fileId, int userId);
    Task<bool> MoveFileAsync(int fileId, int? newFolderId, int userId);
}
