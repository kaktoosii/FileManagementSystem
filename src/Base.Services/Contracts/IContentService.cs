using Base.ViewModels;
using ViewModels.Dto;

namespace Services.Contracts;
public interface IContentService
{
    Task<int> AddNewContentAsync(ContentDto content);
    Task<PagedResponse<List<ContentViewModel>>> GetAllContentPagedAsync(ContentFilterDto filter);
    Task DeleteContentAsync(int id);
    Task<ContentViewModel> FindContentByIdAsync(int contentId);
    Task<ContentDto> GetContentByIdAsync(int contentId);
    Task UpdateContentAsync(ContentDto content);
    Task PublishOrUnpublishContentAsync(int contentId);
}
