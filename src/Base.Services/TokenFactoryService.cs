using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Base.Common.Features.Identity;
using Base.Common.SiteOptions;
using Base.DomainClasses;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Contracts;
using System.Text.Json;

namespace Base.Services;

public class TokenFactoryService : ITokenFactoryService
{
    private readonly IOptionsSnapshot<BearerTokensOptions> _configuration;
    private readonly IDeviceDetectionService _deviceDetectionService;
    private readonly ILogger<TokenFactoryService> _logger;
    private readonly IRolesService _rolesService;
    private readonly ISecurityService _securityService;
    private readonly IUserClaimsService _userClaimsService;

    public TokenFactoryService(
        ISecurityService securityService,
        IRolesService rolesService,
        IOptionsSnapshot<BearerTokensOptions> configuration,
        ILogger<TokenFactoryService> logger,
        IDeviceDetectionService deviceDetectionService,
        IUserClaimsService userClaimsService)
    {
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _rolesService = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deviceDetectionService = deviceDetectionService ?? throw new ArgumentNullException(nameof(deviceDetectionService));
        _userClaimsService = userClaimsService ?? throw new ArgumentNullException(nameof(userClaimsService));
    }

    public async Task<JwtTokensData> CreateJwtTokensAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var (accessToken, claims) = await CreateAccessTokenAsync(user);
        var (refreshTokenValue, refreshTokenSerial) = CreateRefreshToken();
        var permissionsToken = await CreateDynamicClientPermissionsTokenAsync(user);
        return new JwtTokensData
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            DynamicPermissionsToken = permissionsToken,
            RefreshTokenSerial = refreshTokenSerial,
            Claims = claims
        };
    }

    public async Task<string> CreateDynamicClientPermissionsTokenAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var permissions = await _userClaimsService.GetUserClaimsAsync(user.Id, CustomPolicies.DynamicServerPermission);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Iss, _configuration.Value.Issuer, ClaimValueTypes.String, _configuration.Value.Issuer),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, _configuration.Value.Issuer),
            new(CustomPolicies.DynamicClientPermission, JsonSerializer.Serialize(permissions), ClaimValueTypes.String, _configuration.Value.Issuer),
            new(ClaimTypes.UserData, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String, _configuration.Value.Issuer)
        };
        return CreateJwtSecurityToken(claims, _configuration.Value.AccessTokenExpirationMinutes);
    }

    public string CreateJwtSecurityToken(IList<Claim> claims, int expirationMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            _configuration.Value.Issuer,
            _configuration.Value.Audience,
            claims,
            now,
            now.AddMinutes(expirationMinutes),
            credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GetRefreshTokenSerial(string refreshTokenValue)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            return null;
        }

        ClaimsPrincipal decodedRefreshTokenPrincipal = null;

        try
        {
            decodedRefreshTokenPrincipal = new JwtSecurityTokenHandler().ValidateToken(refreshTokenValue,
                new TokenValidationParameters
                {
                    ValidIssuer = _configuration.Value.Issuer,
                    ValidAudience = _configuration.Value.Audience,
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key)),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate refreshTokenValue: `{RefreshTokenValue}`.", refreshTokenValue);
        }

        return decodedRefreshTokenPrincipal?.Claims
            ?.FirstOrDefault(c => string.Equals(c.Type, ClaimTypes.SerialNumber, StringComparison.Ordinal))
            ?.Value;
    }

    private (string RefreshTokenValue, string RefreshTokenSerial) CreateRefreshToken()
    {
        var refreshTokenSerial = _securityService.CreateCryptographicallySecureGuid()
            .ToString()
            .Replace("-", "", StringComparison.Ordinal);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, _securityService.CreateCryptographicallySecureGuid().ToString(), ClaimValueTypes.String, _configuration.Value.Issuer),
            new(JwtRegisteredClaimNames.Iss, _configuration.Value.Issuer, ClaimValueTypes.String, _configuration.Value.Issuer),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, _configuration.Value.Issuer),
            new(ClaimTypes.SerialNumber, refreshTokenSerial, ClaimValueTypes.String, _configuration.Value.Issuer),
            new(ClaimTypes.System, _deviceDetectionService.GetCurrentRequestDeviceDetailsHash(), ClaimValueTypes.String, _configuration.Value.Issuer)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(_configuration.Value.Issuer, _configuration.Value.Audience, claims, now,
            now.AddMinutes(_configuration.Value.RefreshTokenExpirationMinutes), creds);

        var refreshTokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return (refreshTokenValue, refreshTokenSerial);
    }

    private async Task<(string AccessToken, IEnumerable<Claim> Claims)> CreateAccessTokenAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, _securityService.CreateCryptographicallySecureGuid().ToString(), ClaimValueTypes.String, _configuration.Value.Issuer),
            new(JwtRegisteredClaimNames.Iss, _configuration.Value.Issuer, ClaimValueTypes.String, _configuration.Value.Issuer),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, _configuration.Value.Issuer),
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String, _configuration.Value.Issuer),
            new(ClaimTypes.Name, user.Username, ClaimValueTypes.String, _configuration.Value.Issuer),
            new("DisplayName", user.DisplayName ?? "", ClaimValueTypes.String, _configuration.Value.Issuer),
            new("IsCheckDistance", user.IsCheckDistance.ToString(), ClaimValueTypes.Boolean, _configuration.Value.Issuer),
            new("Distance", user.Distance.ToString(CultureInfo.CurrentCulture) , ClaimValueTypes.Integer, _configuration.Value.Issuer),
            new(ClaimTypes.SerialNumber, user.SerialNumber ?? "", ClaimValueTypes.String, _configuration.Value.Issuer),
            new(ClaimTypes.UserData, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String, _configuration.Value.Issuer),
            new(ClaimTypes.System, _deviceDetectionService.GetCurrentRequestDeviceDetailsHash(), ClaimValueTypes.String, _configuration.Value.Issuer)
        };

        var roles = await _rolesService.FindUserRolesAsync(user.Id);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, _configuration.Value.Issuer));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(_configuration.Value.Issuer, _configuration.Value.Audience, claims, now,
            now.AddMinutes(_configuration.Value.AccessTokenExpirationMinutes), creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), claims);
    }
}