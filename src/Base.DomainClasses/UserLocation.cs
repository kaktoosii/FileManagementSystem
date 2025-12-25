namespace Base.DomainClasses;

public class UserLocation
{
    public int UserId { get; set; }
    public int CityId { get; set; }
    public virtual User User { get; set; } = default!;
    public virtual City City { get; set; } = default!;

}
