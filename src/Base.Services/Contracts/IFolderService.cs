using ViewModels.Dto;

namespace Services.Contracts;

public interface IFolderService
{
    Task<int> CreateFolderAsync(CreateFolderDto folderDto, int userId);
    Task<FolderDto> GetFolderByIdAsync(int folderId);
    Task<List<FolderDto>> GetFoldersByUserIdAsync(int userId);
    Task<List<FolderDto>> GetRootFoldersByUserIdAsync(int userId);
    Task<List<FolderDto>> GetSubFoldersAsync(int parentFolderId);
    Task UpdateFolderAsync(int folderId, UpdateFolderDto folderDto, int userId);
    Task DeleteFolderAsync(int folderId, int userId);
    Task MoveFolderAsync(int folderId, int? newParentFolderId, int userId);
    Task<string> GetFolderPathAsync(int folderId);
}
