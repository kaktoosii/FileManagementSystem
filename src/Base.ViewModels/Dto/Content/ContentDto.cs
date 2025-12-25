namespace ViewModels.Dto;
public class ContentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "en"; // Default Language
    public int? ImageId { get; set; }
    public int CategoryId { get; set; }
    public int AuthorId { get; set; }
    public DateTime PublishedDate { get; set; }
    public bool IsPublished { get; set; }
    public int Priority { get; set; }
    public int ContentGroupId { get; set; }
}
