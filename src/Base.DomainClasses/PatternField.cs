using Base.DomainClasses.Enums;
using System;

namespace Base.DomainClasses;

public class PatternField
{
    protected PatternField() => CreateDate = DateTime.UtcNow;

    public PatternField(int filePatternId, string fieldName, string placeholder, FieldType fieldType, int order) : this()
    {
        FilePatternId = filePatternId;
        FieldName = fieldName;
        Placeholder = placeholder;
        FieldType = fieldType;
        Order = order;
    }

    public int Id { get; private set; }
    public int FilePatternId { get; private set; }
    public string FieldName { get; private set; } // Variable name in pattern like {fieldName}
    public string Placeholder { get; private set; } // Display label
    public FieldType FieldType { get; private set; }
    public string Options { get; private set; } // JSON string for Select type options
    public bool IsRequired { get; private set; }
    public string DefaultValue { get; private set; }
    public int Order { get; private set; }
    public DateTime CreateDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation property
    public FilePattern FilePattern { get; private set; } = null!;

    public void UpdateFieldName(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("نام فیلد نمی‌تواند خالی باشد", nameof(fieldName));
        FieldName = fieldName;
    }

    public void UpdatePlaceholder(string placeholder)
    {
        if (string.IsNullOrWhiteSpace(placeholder))
            throw new ArgumentException("Placeholder نمی‌تواند خالی باشد", nameof(placeholder));
        Placeholder = placeholder;
    }

    public void UpdateFieldType(FieldType fieldType)
    {
        FieldType = fieldType;
    }

    public void UpdateOptions(string options)
    {
        Options = options;
    }

    public void SetRequired(bool isRequired)
    {
        IsRequired = isRequired;
    }

    public void UpdateDefaultValue(string defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public void UpdateOrder(int order)
    {
        Order = order;
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
