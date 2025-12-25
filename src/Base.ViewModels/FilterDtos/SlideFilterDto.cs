namespace Base.ViewModels;
public class SlideFilterDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SearchText { get; set; }
    public int? sliderId { get; set; }
    public SlideFilterDto()
    {
        this.PageNumber = 1;
        this.PageSize = 10;
    }
    public SlideFilterDto(int pageNumber, int pageSize, string? searchText, int? sliderId)
    {
        this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
        this.PageSize = pageSize;
        this.SearchText = searchText;
        this.sliderId = sliderId;
    }
}