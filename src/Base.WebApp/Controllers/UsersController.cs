using System.ComponentModel;
using System.Security.Claims;
using Base.Common.Features.Identity;
using Base.DomainClasses;
using Base.Services;
using Base.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using ViewModels.Dto;

namespace Base.WebApp.Controllers;
[DisplayName("مدیریت کاربران")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v1/[controller]"), EnableCors("CorsPolicy"), Authorize(Policy = CustomPolicies.DynamicServerPermission)]
public class UsersController : Controller
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
    }
    [DisplayName("ذخیره اطلاعات کاربر")]
    [HttpPost(), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Post([FromBody] RegisterUserDto model)
    {
        var userId = await _usersService.RegisterUser(model);
        return Ok(new Response<int>
        {
            Data = userId,
            Succeeded = true
        });
    }
    [DisplayName("ویرایش اطلاعات کاربر")]
    [HttpPut("{UserId:int}"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Put(int UserId,[FromBody] EditUserDto model)
    {
        if (model == null)
            throw new KeyNotFoundException("یافت نشد");
        model.Id = UserId;
        await _usersService.UpdateUserAsync(model);
        return Ok(new Response<bool>
        {
            Succeeded = true
        });
    }
    [DisplayName("دریافت اطلاعات کاربر")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<List<UserViewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(UserFilterDto filter)
    {
        var pagedResponse = await _usersService.UserList(filter);
        return Ok(pagedResponse);
    }
    [DisplayName("حذف اطلاعات کاربر")]
    [HttpDelete("{UserId:int}"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Delete(int UserId)
    {
        await _usersService.DeleteUserAsync(UserId);
        return Ok(new Response<bool>
        {
            Succeeded = true
        });
    }
    [DisplayName("دریافت اطلاعات کاربر")]
    [HttpGet("{UserId:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAsync(int UserId)
    {
        var user = await _usersService.GetUserDtoAsync(UserId);
        return Ok(new Response<UserDto>
        {
            Data = user,
            Succeeded = true
        });
    }
}