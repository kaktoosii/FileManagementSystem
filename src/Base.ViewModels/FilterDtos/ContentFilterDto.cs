using Base.ViewModels;

namespace ViewModels.Dto;
public class ContentFilterDto : PaginationFilter
{
    public string? SearchText { get; set; }
    public int? CategoryId { get; set; }

    public ContentFilterDto(int pageNumber, int pageSize, string? searchText, int? categoryId)
        : base(pageNumber, pageSize)
    {
        SearchText = searchText;
        CategoryId = categoryId;
    }
}
