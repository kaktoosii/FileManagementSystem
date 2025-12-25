namespace Base.WebApp.Models;

public class Token
{
    [JsonPropertyName("refreshToken"), Required]
    public string RefreshToken { get; set; }
}