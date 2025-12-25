namespace Base.DomainClasses;

public class User
{
    public int Id { get; set; }

    [MaxLength(450), Required] public string Username { get; set; } = default!;

    [Required] public string Password { get; set; } = default!;

    //public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string ProfileImage { get; set; }
    public string MobileNumber { get; set; }
    public int Confirmcode { get; set; }
    public string FcmToken { get; set; }
    public DateTime? ConfirmcodeDate { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? LastLoggedIn { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }
    public virtual ICollection<UserLocation> UserLocations { get; set; }
    public virtual ICollection<UserToken> UserTokens { get; set; }
    public virtual ICollection<UserClaim> UserClaims { get; set; } = new HashSet<UserClaim>();

    public string SerialNumber { get; set; }
    public bool IsCheckDistance { get; set; }
    public int Distance { get; set; }
    public string DeviceId { get; set; }
}
