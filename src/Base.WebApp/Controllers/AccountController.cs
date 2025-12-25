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

namespace Base.WebApp.Controllers;
[Authorize(), DisplayName("اطلاعات پروفایل")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v1/[controller]"), EnableCors("CorsPolicy")]
public class AccountController : Controller
{
    private readonly IUsersService _usersService;

    public AccountController(
        IUsersService usersService
        )
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
    }
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(Response<UserViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserInfo()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var user = await _usersService.FindUserByUserNameAsync(claimsIdentity?.Name);

        return Json(new Response<UserViewModel>
        {
            Data = new UserViewModel
            {
                Username = claimsIdentity?.Name,
                DisplayName = user.DisplayName,
                ProfileImage = user.ProfileImage,
                MobileNumber = user.MobileNumber,
                IsCheckDistance = user.IsCheckDistance,
                Distance = user.Distance,
                DeviceId = user.DeviceId
            },
            Succeeded = true
        });
    }
    [HttpPut(), IgnoreAntiforgeryToken]
    public async Task<IActionResult> EditUser([FromBody] UserUpdateDto model)
    {
        if (model == null)
            throw new KeyNotFoundException("وارد کردن موارد ضروری الزامیست");
        var claimsIdentity = User.Identity as ClaimsIdentity;
        await _usersService.UpdateProfileAsync(claimsIdentity?.Name, model);
        return Ok();

    }
}