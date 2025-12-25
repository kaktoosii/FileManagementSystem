using System.Security.Claims;

namespace Base.Services;

public class JwtTokensData
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string DynamicPermissionsToken { set; get; }
    public string RefreshTokenSerial { get; set; }
    public IEnumerable<Claim> Claims { get; set; }
}