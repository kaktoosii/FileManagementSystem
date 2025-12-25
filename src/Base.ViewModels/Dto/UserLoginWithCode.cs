namespace Base.DomainClasses;

public class UserLoginWithCode
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public List<int> CityIds { get; set; } = new List<int>();

}