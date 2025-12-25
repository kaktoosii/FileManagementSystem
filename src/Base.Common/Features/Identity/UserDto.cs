namespace Base.Common.Features.Identity;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MobileNumber { get; set; }
    public bool IsCheckDistance { get; set; }
    public int Distance { get; set; }
    public string DeviceId { get; set; }

}