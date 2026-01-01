using System;
using Microsoft.AspNetCore.Http;

namespace ViewModels.Dto;

public class FileDto
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime RegisterDate { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int? FolderId { get; set; }
    public string? FolderName { get; set; }
    public int? FilePatternId { get; set; }
    public string? FilePatternName { get; set; }
    public string? PatternValues { get; set; }
}

public class UploadFileDto
{
    public IFormFile File { get; set; } = null!;
    public int? FolderId { get; set; }
    public int? FilePatternId { get; set; }
    public Dictionary<string, string>? PatternValues { get; set; }
}
