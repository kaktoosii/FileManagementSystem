namespace Base.ViewModels;

public class Response<T>
{
    public Response()
    {
    }
    public Response(T data)
    {
        Succeeded = true;
        Message = string.Empty;
        DeveloperMessages = null;
        Data = data;
    }
    public T? Data { get; set; }
    public bool Succeeded { get; set; }
    public string[]? DeveloperMessages { get; set; }
    public string? Message { get; set; }
}
