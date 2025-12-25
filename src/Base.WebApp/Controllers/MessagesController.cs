using Base.Common.Features.Identity;
using Base.Common.Helpers;
using Base.Services;
using Base.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Services;
using Services.Contracts;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using ViewModels;
using ViewModels.Dto;
using ViewModels.Settings;

namespace WebAPI.Controllers;

[Route("api/v1/[controller]"), EnableCors("CorsPolicy"), Authorize(Policy = CustomPolicies.DynamicServerPermission)]
[DisplayName("مدیریت پیام ها")]
public class MessagesController : Controller
{
    private readonly IMessageService _messageService;
    private readonly ISmsSender _smsSender;
    private readonly ISettingService _settingService;
    private readonly IUsersService _userManager;
    public MessagesController(IMessageService messageService, ISmsSender smsSender, ISettingService settingService, IUsersService usersService)
    {
        _messageService = messageService;
        _smsSender = smsSender;
        _settingService = settingService;
        _userManager = usersService;
    }

    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddNewMessage([FromBody] MessageDto messageDto)
    {
        if(messageDto==null)
            throw new AppException("لطفاً اطلاعات پیام را وارد کنید.");
        //var list = await _userManager.GetAllFcmTokenAsync();
        var setting = await _settingService.GetSettingAsync();
        WebPushConfig push = new WebPushConfig
        {
            webPushFcmSenderId = setting.FcmSenderId,
            webPushFcmServerKey = setting.FcmServerKey,
            icon = setting.Icon
        };
        WebPush webPush = new WebPush();
        webPush.Data = new JObject();
        webPush.Data.title = messageDto.Subject;
        webPush.Data.message = messageDto.Description;
        webPush.Data.icon = setting.Icon;
        await _smsSender.SendWebPushWithFcmToTopicAsync(push, null, webPush.title, webPush.message, setting.FcmServerKey, setting.FcmSenderId);
        var id = await _messageService.AddNewMessageAsync(messageDto);
        return Ok(new { Success = true, MessageId = id });
    }

    [HttpGet]
    public async Task<ActionResult<List<MessageViewModel>>> GetAllMessages(MessageFilterDto filter)
    {
        var messages = await _messageService.GetMessages(filter);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MessageViewModel>> GetMessageById(int id)
    {
        var message = await _messageService.GetMessage(id);
        return Ok(message);
    }

    [HttpPut("{id}"),IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateMessage(int id, [FromBody] MessageViewModel messageDto)
    {
        await _messageService.EditMessageAsync(id, messageDto);
        return Ok(new { Success = true });
    }

    [HttpDelete("{id}"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        await _messageService.DeleteMessageAsync(id);
        return Ok(new { Success = true });
    }
}
