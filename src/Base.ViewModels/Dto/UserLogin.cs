namespace Base.DomainClasses;

public class UserLogin
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Password { get; set; }
    public string? Token { get; set; }
    public string? DeviceId { get; set; }
}