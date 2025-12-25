using System.Security.Claims;

namespace Base.Services;

public interface IAntiForgeryCookieService
{
    void RegenerateAntiForgeryCookies(IEnumerable<Claim> claims);
    void DeleteAntiForgeryCookies();
}