namespace ViewModels.Dto;

public class CreateContentDTO
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ContentBody { get; set; }
    public string? MetaTags { get; set; }
    public string? LanguageCode { get; set; }
    public int PictureId { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishDate { get; set; }
    public string? Tags { get; set; }
    public int ContentGroupId { get; set; }
    public string? VideoUrl { get; set; }
}
