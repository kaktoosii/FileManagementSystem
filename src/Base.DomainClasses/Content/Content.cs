using Base.DomainClasses;

namespace Base.DomainClasses;
public class Content : BaseModel
{
    public string Title { get; private set; }
    public string Summary { get; private set; }
    public string Body { get; private set; }
    public string LanguageCode { get; private set; }
    public int? ImageId { get; private set; }
    public int CategoryId { get; private set; }
    public int AuthorId { get; private set; }
    public DateTime PublishedDate { get; private set; }
    public bool IsPublished { get; private set; }
    public int Priority { get; private set; }
    public int ContentGroupId { get; private set; } // Foreign Key

    // Navigation Property
    public ContentGroup ContentGroup { get; private set; } = default!;

    public Content(string title, string summary, string body, string languageCode, int? imageId,
                   int categoryId, int authorId, DateTime publishedDate, bool isPublished,
                   int priority, int contentGroupId)
    {
        Title = title;
        Summary = summary;
        Body = body;
        LanguageCode = languageCode;
        ImageId = imageId;
        CategoryId = categoryId;
        AuthorId = authorId;
        PublishedDate = publishedDate;
        IsPublished = isPublished;
        Priority = priority;
        ContentGroupId = contentGroupId;
    }

    public void Update(string title, string summary, string body, string languageCode, int? imageId,
                       int categoryId, int authorId, DateTime publishedDate, bool isPublished,
                       int priority)
    {
        Title = title;
        Summary = summary;
        Body = body;
        LanguageCode = languageCode;
        ImageId = imageId;
        CategoryId = categoryId;
        AuthorId = authorId;
        PublishedDate = publishedDate;
        IsPublished = isPublished;
        Priority = priority;
    }

    public void TogglePublishStatus()
    {
        IsPublished = !IsPublished;
    }
}
