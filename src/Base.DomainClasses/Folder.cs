using System;
using System.Collections.Generic;

namespace Base.DomainClasses;

public class Folder
{
    protected Folder() => CreateDate = DateTime.UtcNow;

    public Folder(string name, int? parentFolderId, int userId) : this()
    {
        Name = name;
        ParentFolderId = parentFolderId;
        UserId = userId;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int? ParentFolderId { get; private set; }
    public int UserId { get; private set; }
    public DateTime CreateDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public Folder ParentFolder { get; private set; }
    public List<Folder> SubFolders { get; private set; } = new List<Folder>();
    public List<Base.DomainClasses.File> Files { get; private set; } = new List<Base.DomainClasses.File>();

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام فولدر نمی‌تواند خالی باشد", nameof(name));
        Name = name;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void MoveToFolder(int? parentFolderId)
    {
        ParentFolderId = parentFolderId;
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
