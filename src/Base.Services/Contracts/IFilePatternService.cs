using ViewModels.Dto;

namespace Services.Contracts;

public interface IFilePatternService
{
    Task<int> CreateFilePatternAsync(CreateFilePatternDto patternDto, int userId);
    Task<FilePatternDto> GetFilePatternByIdAsync(int patternId);
    Task<List<FilePatternDto>> GetFilePatternsByUserIdAsync(int userId);
    Task UpdateFilePatternAsync(int patternId, UpdateFilePatternDto patternDto, int userId);
    Task DeleteFilePatternAsync(int patternId, int userId);
    Task<string> GenerateFileNameAsync(int patternId, Dictionary<string, string> fieldValues);
}
