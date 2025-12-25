using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Base.Services;

public interface ITokenValidatorService
{
    Task ValidateAsync(TokenValidatedContext context);
}