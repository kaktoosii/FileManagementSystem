using System.ComponentModel;
using System.Security.Claims;
using Base.Common.Features.Identity;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.Services;
using Base.ViewModels;
using Base.ViewModels.Dto;
using Base.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using ViewModels;

namespace Base.WebApp.Controllers;
[DisplayName("اطلاعات پروفایل"), Authorize(Policy = CustomPolicies.DynamicServerPermission)]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v1/[controller]"), EnableCors("CorsPolicy")]
public class SettingsController : Controller
{
    private readonly ISettingService _settingService;

    public SettingsController(
        ISettingService settingService
        )
    {
        _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
    }
    [HttpGet()]
    [ProducesResponseType(typeof(Response<SettingViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSetting()
    {
        var setting = await _settingService.GetSettingAsync();

        return Json(new Response<SettingViewModel>
        {
            Data = setting,
            Succeeded = true
        });
    }
    [HttpPut(), IgnoreAntiforgeryToken]
    public async Task<IActionResult> EditSetting([FromBody] SettingViewModel model)
    {
        if (model == null)
            throw new KeyNotFoundException("وارد کردن موارد ضروری الزامیست");
        await _settingService.UpdateSettingAsync(model);
        return Ok();

    }
}