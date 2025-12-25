namespace Base.ViewModels;

public class PagedResponse<T> : Response<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public dynamic? ExtraData { get; set; }

    public PagedResponse(T data, int pageNumber, int pageSize, int totalRecords, dynamic? extraData =null)
    {
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.Data = data;
        this.Message = string.Empty;
        this.Succeeded = true;
        this.DeveloperMessages = null;
        this.TotalRecords = totalRecords;
        this.ExtraData = extraData;
    }
}