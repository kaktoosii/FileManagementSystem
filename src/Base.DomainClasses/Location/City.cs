namespace Base.DomainClasses;

public class City
{
    public City(string title)
    {
        this.Title = title;
    }
    public int Id { get; set; }
    [StringLengthAttribute(500)]
    public string Title { get; set; }
    public virtual United United { get; set; }
    public virtual ICollection<UserLocation> UserLocations { get; set; }
}
