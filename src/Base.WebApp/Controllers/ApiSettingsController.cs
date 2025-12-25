using Base.Common.Features.Identity;
using Base.DomainClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel;

namespace Base.WebApp.Controllers;

[Authorize(Policy = CustomPolicies.DynamicServerPermission),
 Route("api/[controller]"),
 EnableCors("CorsPolicy"),
 DisplayName("تنظیمات")]
public class ApiSettingsController : Controller
{
    private readonly IOptionsSnapshot<ApiSettings> _apiSettingsConfig;

    public ApiSettingsController(IOptionsSnapshot<ApiSettings> apiSettingsConfig)
    {
        _apiSettingsConfig = apiSettingsConfig ?? throw new ArgumentNullException(nameof(apiSettingsConfig));
    }


    [DisplayName("دریافت تنظیمان")]
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_apiSettingsConfig.Value); // For the Angular Client
    }


    [DisplayName("بروزرسانی تنظیمان")]
    [HttpPut]
    public IActionResult Put()
    {
        return Ok(); // For the Angular Client
    }
}