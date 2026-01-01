using System;
using System.Collections.Generic;

namespace Base.DomainClasses;

public class FilePattern
{
    protected FilePattern() => CreateDate = DateTime.UtcNow;

    public FilePattern(string name, string pattern, int userId) : this()
    {
        Name = name;
        Pattern = pattern;
        UserId = userId;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Pattern { get; private set; } // Pattern template like "{field1}_{field2}_{date}"
    public int UserId { get; private set; }
    public DateTime CreateDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public List<PatternField> Fields { get; private set; } = new List<PatternField>();
    public List<File> Files { get; private set; } = new List<File>();

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام پترن نمی‌تواند خالی باشد", nameof(name));
        Name = name;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void UpdatePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("پترن نمی‌تواند خالی باشد", nameof(pattern));
        Pattern = pattern;
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
