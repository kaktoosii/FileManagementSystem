using System;

namespace Base.DomainClasses;

public class File
{
    protected File() => RegisterDate = DateTime.UtcNow;

    public File(
        string path,
        string fileName,
        int userId,
        string uploaderIp,
        string mimeType,
        long fileSize,
        int? folderId = null,
        int? filePatternId = null,
        string patternValues = null
    ) : this()
    {
        Path = path;
        FileName = fileName;
        UserId = userId;
        UploaderIp = uploaderIp;
        MimeType = mimeType;
        FileSize = fileSize;
        FolderId = folderId;
        FilePatternId = filePatternId;
        PatternValues = patternValues;
    }

    public int Id { get; private set; }
    public string Path { get; private set; }
    public string FileName { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public string UploaderIp { get; private set; }
    public DateTime RegisterDate { get; private set; }
    public string MimeType { get; private set; }
    public long FileSize { get; private set; }
    public int? FolderId { get; private set; }
    public int? FilePatternId { get; private set; }
    public string PatternValues { get; private set; } // JSON string of field values used
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public Folder Folder { get; private set; }
    public FilePattern FilePattern { get; private set; }

    public void UpdateOriginalFileName(string originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("نام فایل اصلی نمی‌تواند خالی باشد", nameof(originalFileName));
        OriginalFileName = originalFileName;
    }

    public void UpdateFolder(int? folderId)
    {
        FolderId = folderId;
    }

    public void UpdatePattern(int? filePatternId, string patternValues)
    {
        FilePatternId = filePatternId;
        PatternValues = patternValues;
    }

    public void UpdateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("نام فایل نمی‌تواند خالی باشد", nameof(fileName));
        FileName = fileName;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }
}
