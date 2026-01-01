using System;
using System.Collections.Generic;
using Base.DomainClasses;
using Base.DomainClasses.Enums;

namespace ViewModels.Dto;

public class FilePatternDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime CreateDate { get; set; }
    public List<PatternFieldDto> Fields { get; set; } = new List<PatternFieldDto>();
}

public class PatternFieldDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public string? Options { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public int Order { get; set; }
}

public class CreateFilePatternDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public List<CreatePatternFieldDto> Fields { get; set; } = new List<CreatePatternFieldDto>();
}

public class CreatePatternFieldDto
{
    public string FieldName { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public string? Options { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public int Order { get; set; }
}

public class UpdateFilePatternDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public List<CreatePatternFieldDto> Fields { get; set; } = new List<CreatePatternFieldDto>();
}

public class PatternFieldValueDto
{
    public string FieldName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class UploadDocumentWithPatternDto
{
    public Microsoft.AspNetCore.Http.IFormFile File { get; set; } = null!;
    public int? FolderId { get; set; }
    public int? FilePatternId { get; set; }
    public Dictionary<string, string>? PatternValues { get; set; }
}
