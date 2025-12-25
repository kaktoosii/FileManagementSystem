namespace Base.DomainClasses;

public class ConfirmCode
{

    [Required]
    public string? Username { get; set; }
    [Required]
    public int ConfirmCodeNumber { get; set; }

    
}