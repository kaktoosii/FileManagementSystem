namespace Base.IntegrationTests.Models;

public class Token
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; set; }

    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; }
}