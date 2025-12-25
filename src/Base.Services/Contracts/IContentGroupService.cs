using Base.ViewModels;
using ViewModels.Dto;

namespace Services.Contracts;
public interface IContentGroupService
{
    Task<int> AddNewContentGroupAsync(ContentGroupDto groupDto);
    Task<List<ContentGroupViewModel>> GetAllContentGroupsAsync();
    Task<ContentGroupViewModel> GetContentGroupByIdAsync(int id);
    Task UpdateContentGroupAsync(ContentGroupDto groupDto);
    Task DeleteContentGroupAsync(int id);
}
