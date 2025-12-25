using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using ViewModels.Dto;

namespace Base.WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContentController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddContent([FromBody] ContentDto contentDto)
    {
        var contentId = await _contentService.AddNewContentAsync(contentDto);
        return Ok(new { message = "Content successfully added", contentId });
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetContentById(int id)
    {
        var content = await _contentService.GetContentByIdAsync(id);
        return Ok(content);
    }


    [HttpPost("list")]
    public async Task<IActionResult> GetPagedContent([FromBody] ContentFilterDto filter)
    {
        var pagedContents = await _contentService.GetAllContentPagedAsync(filter);
        return Ok(pagedContents);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateContent([FromBody] ContentDto contentDto)
    {
        await _contentService.UpdateContentAsync(contentDto);
        return Ok(new { message = "Content successfully updated" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteContent(int id)
    {
        await _contentService.DeleteContentAsync(id);
        return Ok(new { message = "Content deleted" });
    }

    [HttpPatch("publish/{id}")]
    public async Task<IActionResult> PublishContent(int id)
    {
        await _contentService.PublishOrUnpublishContentAsync(id);
        return Ok(new { message = "Publish status changed" });
    }
}
