

using Base.DomainClasses;

namespace Base.Services;

public interface ITokenFactoryService
{
    Task<JwtTokensData> CreateJwtTokensAsync(User user);
    string GetRefreshTokenSerial(string refreshTokenValue);
}