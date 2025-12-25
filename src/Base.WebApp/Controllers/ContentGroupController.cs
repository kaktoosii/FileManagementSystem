using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using ViewModels.Dto;
using Base.ViewModels;
using Microsoft.Extensions.Localization;

namespace Base.WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContentGroupController : ControllerBase
{
    private readonly IContentGroupService _contentGroupService;
    private readonly IStringLocalizer<ContentGroupController> _localizer;

    public ContentGroupController(IContentGroupService contentGroupService, IStringLocalizer<ContentGroupController> localizer)
    {
        _contentGroupService = contentGroupService;
        _localizer = localizer;
    }

    [HttpPost()]
    public async Task<IActionResult> AddContentGroup([FromBody] ContentGroupDto groupDto)
    {
        var groupId = await _contentGroupService.AddNewContentGroupAsync(groupDto);
        return Ok(new { message = "Content Group successfully added", groupId });
    }

    [HttpGet()]
    public async Task<IActionResult> GetAllContentGroups()
    {
        var groups = await _contentGroupService.GetAllContentGroupsAsync();
        return Ok(groups);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetContentGroupById(int id)
    {
        var group = await _contentGroupService.GetContentGroupByIdAsync(id);
        if (group == null)
        {
            return NotFound(new { message = _localizer["ContentGroupNotFound"] });
        }
        return Ok(group);
    }

    [HttpPut()]
    public async Task<IActionResult> UpdateContentGroup([FromBody] ContentGroupDto groupDto)
    {
        await _contentGroupService.UpdateContentGroupAsync(groupDto);
        return Ok(new { message = "Content Group successfully updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContentGroup(int id)
    {
        await _contentGroupService.DeleteContentGroupAsync(id);
        return Ok(new { message = "Content Group deleted" });
    }
}
