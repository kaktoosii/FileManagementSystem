using System;
using System.Collections.Generic;

namespace ViewModels.Dto;

public class FolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentFolderId { get; set; }
    public string? ParentFolderName { get; set; }
    public int UserId { get; set; }
    public DateTime CreateDate { get; set; }
    public List<FolderDto>? SubFolders { get; set; }
    public int DocumentCount { get; set; }
}

public class CreateFolderDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentFolderId { get; set; }
}

public class UpdateFolderDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentFolderId { get; set; }
}
