namespace Base.ViewModels;
public class SupportFilterDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SearchText { get; set; }
    public SupportFilterDto()
    {
        this.PageNumber = 1;
        this.PageSize = 10;
    }
    public SupportFilterDto(int pageNumber, int pageSize, string? searchText)
    {
        this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
        this.PageSize = pageSize;
        this.SearchText = searchText;
    }
}