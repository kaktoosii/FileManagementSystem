using Base.DomainClasses;

namespace Base.DomainClasses;
public class ContentGroup : BaseModel
{
    public string GroupName { get; private set; }
    public string Description { get; private set; }

    // Navigation Property
    public ICollection<Content> Contents { get; private set; } = new List<Content>();

    public ContentGroup(string groupName, string description)
    {
        GroupName = groupName;
        Description = description;
    }

    public void Update(string groupName, string description)
    {
        GroupName = groupName;
        Description = description;
    }
}
