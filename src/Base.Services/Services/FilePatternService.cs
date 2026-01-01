using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using System.Text.Json;
using System.Text.RegularExpressions;
using ViewModels.Dto;
using System.Linq;

namespace Services.Services;

public class FilePatternService : IFilePatternService
{
    private readonly IUnitOfWork _uow;
    private readonly DbSet<FilePattern> _filePatterns;
    private readonly DbSet<PatternField> _patternFields;

    public FilePatternService(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _filePatterns = _uow.Set<FilePattern>();
        _patternFields = _uow.Set<PatternField>();
    }

    public async Task<int> CreateFilePatternAsync(CreateFilePatternDto patternDto, int userId)
    {
        if (patternDto == null)
            throw new AppException("پترن نمی‌تواند خالی باشد");

        if (string.IsNullOrWhiteSpace(patternDto.Name))
            throw new AppException("نام پترن نمی‌تواند خالی باشد");

        if (string.IsNullOrWhiteSpace(patternDto.Pattern))
            throw new AppException("پترن نمی‌تواند خالی باشد");

        // Validate pattern syntax - should contain {fieldName} placeholders
        if (!patternDto.Pattern.Contains('{', StringComparison.Ordinal) || !patternDto.Pattern.Contains('}', StringComparison.Ordinal))
            throw new AppException("پترن باید شامل {fieldName} باشد");

        // Check if all fields in pattern are defined
        var patternFields = ExtractFieldNames(patternDto.Pattern);
        if (patternDto.Fields == null || patternDto.Fields.Count == 0)
            throw new AppException("پترن باید حداقل یک فیلد داشته باشد");

        var definedFieldNames = patternDto.Fields.Select(f => f.FieldName).ToHashSet();
        foreach (var fieldName in patternFields)
        {
            if (!definedFieldNames.Contains(fieldName))
                throw new AppException($"فیلد {fieldName} در پترن تعریف شده اما در لیست فیلدها وجود ندارد");
        }

        var filePattern = new FilePattern(patternDto.Name, patternDto.Pattern, userId);
        if (!string.IsNullOrWhiteSpace(patternDto.Description))
            filePattern.UpdateDescription(patternDto.Description);

        await _filePatterns.AddAsync(filePattern);
        await _uow.SaveChangesAsync();

        // Add pattern fields
        foreach (var fieldDto in patternDto.Fields.OrderBy(f => f.Order))
        {
            var patternField = new PatternField(
                filePattern.Id,
                fieldDto.FieldName,
                fieldDto.Placeholder,
                fieldDto.FieldType,
                fieldDto.Order
            );

            if (!string.IsNullOrWhiteSpace(fieldDto.Options))
                patternField.UpdateOptions(fieldDto.Options);

            patternField.SetRequired(fieldDto.IsRequired);

            if (!string.IsNullOrWhiteSpace(fieldDto.DefaultValue))
                patternField.UpdateDefaultValue(fieldDto.DefaultValue);

            await _patternFields.AddAsync(patternField);
        }

        await _uow.SaveChangesAsync();
        return filePattern.Id;
    }

    public async Task<FilePatternDto> GetFilePatternByIdAsync(int patternId)
    {
        var pattern = await _filePatterns
            .FirstOrDefaultAsync(p => p.Id == patternId && !p.IsDeleted);

        if (pattern == null)
            return null;

        var fields = await _patternFields
            .Where(f => f.FilePatternId == patternId && !f.IsDeleted)
            .OrderBy(f => f.Order)
            .ToListAsync();

        return new FilePatternDto
        {
            Id = pattern.Id,
            Name = pattern.Name,
            Description = pattern.Description,
            Pattern = pattern.Pattern,
            UserId = pattern.UserId,
            CreateDate = pattern.CreateDate,
            Fields = fields.Select(f => new PatternFieldDto
            {
                Id = f.Id,
                FieldName = f.FieldName,
                Placeholder = f.Placeholder,
                FieldType = f.FieldType,
                Options = f.Options,
                IsRequired = f.IsRequired,
                DefaultValue = f.DefaultValue,
                Order = f.Order
            }).ToList()
        };
    }

    public async Task<List<FilePatternDto>> GetFilePatternsByUserIdAsync(int userId)
    {
        var patterns = await _filePatterns
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();

        var result = new List<FilePatternDto>();
        foreach (var pattern in patterns)
        {
            var fields = await _patternFields
                .Where(f => f.FilePatternId == pattern.Id && !f.IsDeleted)
                .OrderBy(f => f.Order)
                .ToListAsync();

            result.Add(new FilePatternDto
            {
                Id = pattern.Id,
                Name = pattern.Name,
                Description = pattern.Description,
                Pattern = pattern.Pattern,
                UserId = pattern.UserId,
                CreateDate = pattern.CreateDate,
                Fields = fields.Select(f => new PatternFieldDto
                {
                    Id = f.Id,
                    FieldName = f.FieldName,
                    Placeholder = f.Placeholder,
                    FieldType = f.FieldType,
                    Options = f.Options,
                    IsRequired = f.IsRequired,
                    DefaultValue = f.DefaultValue,
                    Order = f.Order
                }).ToList()
            });
        }

        return result;
    }

    public async Task UpdateFilePatternAsync(int patternId, UpdateFilePatternDto patternDto, int userId)
    {
        if (patternDto == null)
            throw new AppException("پترن نمی‌تواند خالی باشد");

        var pattern = await _filePatterns
            .FirstOrDefaultAsync(p => p.Id == patternId && p.UserId == userId && !p.IsDeleted);

        if (pattern == null)
            throw new AppException("پترن یافت نشد");

        // Validate pattern syntax
        if (!patternDto.Pattern.Contains('{', StringComparison.Ordinal) || !patternDto.Pattern.Contains('}', StringComparison.OrdinalIgnoreCase))
            throw new AppException("پترن باید شامل {fieldName} باشد");

        // Check if all fields in pattern are defined
        var patternFields = ExtractFieldNames(patternDto.Pattern);
        if (patternDto.Fields == null || patternDto.Fields.Count == 0)
            throw new AppException("پترن باید حداقل یک فیلد داشته باشد");

        var definedFieldNames = patternDto.Fields.Select(f => f.FieldName).ToHashSet();
        foreach (var fieldName in patternFields)
        {
            if (!definedFieldNames.Contains(fieldName))
                throw new AppException($"فیلد {fieldName} در پترن تعریف شده اما در لیست فیلدها وجود ندارد");
        }

        pattern.UpdateName(patternDto.Name);
        pattern.UpdateDescription(patternDto.Description);
        pattern.UpdatePattern(patternDto.Pattern);

        // Soft delete existing fields
        var existingFields = await _patternFields
            .Where(f => f.FilePatternId == patternId && !f.IsDeleted)
            .ToListAsync();

        foreach (var field in existingFields)
        {
            field.SoftDelete();
        }

        // Add new fields
        foreach (var fieldDto in patternDto.Fields.OrderBy(f => f.Order))
        {
            var patternField = new PatternField(
                patternId,
                fieldDto.FieldName,
                fieldDto.Placeholder,
                fieldDto.FieldType,
                fieldDto.Order
            );

            if (!string.IsNullOrWhiteSpace(fieldDto.Options))
                patternField.UpdateOptions(fieldDto.Options);

            patternField.SetRequired(fieldDto.IsRequired);

            if (!string.IsNullOrWhiteSpace(fieldDto.DefaultValue))
                patternField.UpdateDefaultValue(fieldDto.DefaultValue);

            await _patternFields.AddAsync(patternField);
        }

        await _uow.SaveChangesAsync();
    }

    public async Task DeleteFilePatternAsync(int patternId, int userId)
    {
        var pattern = await _filePatterns
            .FirstOrDefaultAsync(p => p.Id == patternId && p.UserId == userId && !p.IsDeleted);

        if (pattern == null)
            throw new AppException("پترن یافت نشد");

        pattern.SoftDelete();
        await _uow.SaveChangesAsync();
    }

    public async Task<string> GenerateFileNameAsync(int patternId, Dictionary<string, string> fieldValues)
    {
        var pattern = await _filePatterns
            .FirstOrDefaultAsync(p => p.Id == patternId && !p.IsDeleted);

        if (pattern == null)
            throw new AppException("پترن یافت نشد");

        var fields = await _patternFields
            .Where(f => f.FilePatternId == patternId && !f.IsDeleted)
            .ToListAsync();

        var result = pattern.Pattern;
        if(fieldValues==null)
            throw new AppException("مقادیر خالی است");
        foreach (var (field, placeholder) in
        // Replace each {fieldName} with actual value
        from field in fields
        let placeholder = $"{{{field.FieldName}}}"
        select (field, placeholder))
        {
            fieldValues.TryGetValue(field.FieldName, out string value);
            // Sanitize value for filename (remove invalid characters)
            value = SanitizeFileName(value);
            result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        // Remove any remaining placeholders
        result = result.Replace("{", "", StringComparison.OrdinalIgnoreCase).Replace("}", "", StringComparison.OrdinalIgnoreCase);
        return result;
    }

    private static List<string> ExtractFieldNames(string pattern)
    {
        var regex = new Regex(@"\{([^}]+)\}", RegexOptions.None, TimeSpan.FromSeconds(1));
        var matches = regex.Matches(pattern);
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        // Remove invalid filename characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName
            .Where(c => !invalidChars.Contains(c))
            .ToArray());

        // Replace spaces with underscore
        sanitized = sanitized.Replace(" ", "_", StringComparison.OrdinalIgnoreCase);

        // Limit length
        if (sanitized.Length > 100)
            sanitized = sanitized.Substring(0, 100);

        return sanitized;
    }
}
