using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using Base.Common.Features.Identity;
using Base.Common.Helpers;
using Base.Common.SiteOptions;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.Services;
using Base.ViewModels;
using Base.ViewModels.ViewModel;
using Base.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Services.Contracts;
using ViewModels.Dto;

namespace Base.WebApp.Controllers;
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v1/[controller]"), EnableCors("CorsPolicy")]
public class AuthController : Controller
{
    private readonly IAntiForgeryCookieService _antiforgery;
    private readonly ITokenFactoryService _tokenFactoryService;
    private readonly IUnitOfWork _uow;
    private readonly IUsersService _usersService;
    private readonly HttpClient _httpClient;
    private readonly string _secret;
    private readonly ITokenStoreService _tokenStoreService;
    public AuthController(
        IUsersService usersService,
        ITokenStoreService tokenStoreService,
        ITokenFactoryService tokenFactoryService,
        IUnitOfWork uow,
        IOptionsSnapshot<SiteSettingsDto> configuration,
        HttpClient httpClient,
        IAntiForgeryCookieService antiforgery)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        if (configuration is null || configuration.Value is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        _secret = configuration.Value.Secret?? "";
        _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
        _tokenFactoryService = tokenFactoryService ?? throw new ArgumentNullException(nameof(tokenFactoryService));
        _httpClient = httpClient;
        _tokenStoreService = tokenStoreService ?? throw new ArgumentNullException(nameof(tokenStoreService));
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(UserTokensDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] UserLogin loginUser)
    {
        if (loginUser == null)
        {
            return BadRequest(error: "user is not set.");
        }

        var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password);

        if (user?.IsActive != true)
        {
            return Unauthorized();
        }
        if(!string.IsNullOrWhiteSpace(user.DeviceId) && !string.IsNullOrWhiteSpace(loginUser.DeviceId))
        {
            if (!string.Equals(user.DeviceId, loginUser.DeviceId, StringComparison.Ordinal))
            {
                return Unauthorized();
            }
        }

        var result = await _tokenFactoryService.CreateJwtTokensAsync(user);

        await _tokenStoreService.AddUserTokenAsync(user, result.RefreshTokenSerial, result.AccessToken,
            refreshTokenSourceSerial: null);

        await _uow.SaveChangesAsync();

        _antiforgery.RegenerateAntiForgeryCookies(result.Claims);
        Response.Cookies.Append("auth_token", result.AccessToken, new CookieOptions
        {
            HttpOnly = true, // Prevent JavaScript access to the cookie
            Secure = true,   // Set to true for production (requires HTTPS)
            SameSite = SameSiteMode.Lax, // Prevent CSRF
            Expires = DateTime.UtcNow.AddDays(7) // Set the expiration of the cookie
        });
        return Ok(new UserTokensDto
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            DynamicPermissionsToken = result.DynamicPermissionsToken
        });
        //return Ok(new Response<UserTokensDto>
        //{
        //    Data = new UserTokensDto
        //    {
        //        AccessToken = result.AccessToken,
        //        RefreshToken = result.RefreshToken
        //    },
        //    Succeeded = true
        //});
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("RefreshToken")]
    [ProducesResponseType(typeof(PagedResponse<UserTokensDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] Token model)
    {
        if (model == null)
        {
            return BadRequest();
        }

        var refreshTokenValue = model.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            return BadRequest(error: "refreshToken is not set.");
        }

        var token = await _tokenStoreService.FindTokenAsync(refreshTokenValue);

        if (token == null)
        {
            return Unauthorized(value: "This is not our token!");
        }

        var result = await _tokenFactoryService.CreateJwtTokensAsync(token.User);

        await _tokenStoreService.AddUserTokenAsync(token.User, result.RefreshTokenSerial, result.AccessToken,
            _tokenFactoryService.GetRefreshTokenSerial(refreshTokenValue));

        await _uow.SaveChangesAsync();

        _antiforgery.RegenerateAntiForgeryCookies(result.Claims);

        return Ok(new UserTokensDto
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        });
    }

    [AllowAnonymous]
    [HttpGet(template: "[action]")]
    public async Task<bool> Logout(string refreshToken)
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var userIdValue = claimsIdentity?.FindFirst(ClaimTypes.UserData)?.Value;

        // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
        // Delete the user's tokens from the database (revoke its bearer token)
        await _tokenStoreService.RevokeUserBearerTokensAsync(userIdValue, refreshToken);
        await _uow.SaveChangesAsync();

        _antiforgery.DeleteAntiForgeryCookies();
        Response.Cookies.Delete("auth_token");
        return true;
    }







    [HttpPost("[action]"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
    {
        if (model == null)
        {
            throw new AppException("اطلاعات ارسالی نامعتبر است");
        }

        var user = await _usersService.GetCurrentUserAsync();
        if (user == null)
        {
            throw new AppException("کاربر یافت نشد");
        }
        if (model.NewPassword.Length < 8)
            throw new AppException("رمز عبور باید حداقل 8 حرف باشد.");
        var (succeeded, error) = await _usersService.ChangePasswordAsync(user,model.OldPassword,model.NewPassword);
        return succeeded ? Ok(new ApiResponseDto { Success = true }) : throw new AppException(error);

    }

    [AllowAnonymous, IgnoreAntiforgeryToken, HttpPost("[action]")]
    public async Task<IActionResult> LoginWithCode([FromBody] UserLoginWithCode loginUser)
    {
        if (loginUser == null)
        {
            throw new AppException("اطلاعات ارسالی نامعتبر است");
        }

        var user = await _usersService.FindUserByUserNameAsync(loginUser.Username);
        if (user == null)
        {
            
            List<Role> roles = new List<Role>();
            roles.Add(new Role
            {
                Name = CustomRoles.User
            });
            var userModel = new User()
            {
                Username = loginUser.Username,
                Password = "FDrer$356%s",
                MobileNumber = loginUser.Username,
                IsActive = true,
            };
            user = await _usersService.AddUser(userModel, roles,loginUser.CityIds);
        }
        if (!user.IsActive)
        {
            return BadRequest("این کاربر غیر فعال است.");
        }

        await _usersService.ConfirmCode(loginUser.Username);
        return Ok(new Response<bool>
        {
            Message = "یک پیامک حاوی کد تایید برای شما ارسال شد.",
            Succeeded = true
        });
    }
    [NonAction]
    public async Task<bool> IsRecaptchV3Valid(string captchaResponseToken)
    {
        try
        {
            var verifyUrl = "https://www.google.com/recaptcha/api/siteverify";
            var parameters = new Dictionary<string, string>
            {
                {"secret",  _secret},
                {"response", captchaResponseToken}
                //{"remoteip", "ip" } <= this is optional
            };
            using (HttpContent formContent = new FormUrlEncodedContent(parameters))
            {
                using (var response = await _httpClient.PostAsync(verifyUrl, formContent).ConfigureAwait(false))
                {
                    string gRecaptchaJsonresult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    RecaptchaResponse grcV3Response = JsonConvert.DeserializeObject<RecaptchaResponse>(gRecaptchaJsonresult);

                    if (grcV3Response.score < 0.5)
                        return false;

                    return grcV3Response!.success;
                    

                }
            }
        }
        catch (Exception ex)
        {
            throw new AppException(ex.Message);
        }
    }
    [AllowAnonymous, IgnoreAntiforgeryToken, HttpPost("[action]")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgetPassword model)
    {
        if (model == null)
        {
            throw new AppException("نام کاربری الزامیست");
        }

        var user = await _usersService.FindUserByUserNameAsync(model.Username);
        if (user?.IsActive != true)
        {
            return BadRequest("کاربری شما غیر فعال است.");
        }
        await _usersService.ConfirmCode(model.Username);
        return Ok(new Response<bool>
        {
            Message = "یک پیامک حاوی کد تایید برای شما ارسال شد.",
            Succeeded = true
        });
    }
    [AllowAnonymous, IgnoreAntiforgeryToken, HttpPost("[action]")]
    public async Task<IActionResult> SaveNewPassword([FromBody] ChangePassword model)
    {
        if (model == null)
        {
            throw new AppException("نام کاربری الزامیست");
        }
       
        var (succeeded, error) = await _usersService.ForgetPasswordAsync(new ChangePassword
        {
            NewPassword = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword,
            Token = model.Token
        });
        List<string> errors = new List<string>();
        errors.Add(error);
        return Ok(new Response<bool>
        {
            DeveloperMessages = errors.ToArray(),
            Succeeded = succeeded
        });
    }
    [AllowAnonymous, IgnoreAntiforgeryToken, HttpPost("[action]")]
    [ProducesResponseType(typeof(UserTokensDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmUser([FromBody] ConfirmCode model)
    {
        if (model == null)
        {
            throw new AppException("نام کاربری الزامیست");
        }
        if (model.Username == null || model.ConfirmCodeNumber == 0)
        {
            throw new AppException("نام کاربری الزامیست");
        }
        var user = await _usersService.ConfirmUser(model);
        var result = await _tokenFactoryService.CreateJwtTokensAsync(user);

        await _tokenStoreService.AddUserTokenAsync(user, result.RefreshTokenSerial, result.AccessToken,
            refreshTokenSourceSerial: null);

        await _uow.SaveChangesAsync();

        Response.Cookies.Append("auth_token", result.AccessToken, new CookieOptions
        {
            HttpOnly = true, // Prevent JavaScript access to the cookie
            Secure = false,   // Set to true for production (requires HTTPS)
            SameSite = SameSiteMode.Lax, // Prevent CSRF
            Expires = DateTime.UtcNow.AddDays(7), // Set the expiration of the cookie
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(7),
        });
        //return Ok(new Response<UserTokensDto>
        //{
        //    Data = new UserTokensDto
        //    {
        //        AccessToken = result.AccessToken,
        //        RefreshToken = result.RefreshToken
        //    },
        //    Succeeded = true
        //});
        return Ok(new UserTokensDto
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        });
    }

  
}