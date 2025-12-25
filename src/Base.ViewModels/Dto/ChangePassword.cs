namespace Base.DomainClasses;

public class ChangePassword
{
    [Required(ErrorMessage = "(*)")] public string Token { get; set; } = default!;
    [Required(ErrorMessage = "(*)")] public string NewPassword { get; set; } = default!;

    [Required(ErrorMessage = "(*)"), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = default!;
}