namespace Base.DomainClasses;

public class United
{
    public int Id { get; set; }
    [StringLengthAttribute(500)]
    public string Title { get; set; }
}
