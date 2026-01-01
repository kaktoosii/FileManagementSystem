using Base.Common.Helpers;
using Common.IdentityToolkit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using ViewModels.Dto;

namespace Areas.Identity.Controllers;

[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v1/[controller]"), EnableCors("CorsPolicy"), Authorize()]
public class FilePatternController : Controller
{
    private readonly IFilePatternService _filePatternService;

    public FilePatternController(IFilePatternService filePatternService)
    {
        _filePatternService = filePatternService;
    }

    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<ActionResult> Create([FromBody] CreateFilePatternDto patternDto)
    {
        if (patternDto == null)
            throw new AppException("پترن نمی‌تواند خالی باشد");

        var userId = User.Identity.GetUserId();
        var patternId = await _filePatternService.CreateFilePatternAsync(patternDto, userId);
        return Ok(new { message = "پترن با موفقیت ایجاد شد", patternId });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var pattern = await _filePatternService.GetFilePatternByIdAsync(id);
        if (pattern == null)
            return NotFound("پترن یافت نشد");

        var userId = User.Identity.GetUserId();
        if (pattern.UserId != userId)
            return Forbid("شما دسترسی به این پترن ندارید");

        return Ok(pattern);
    }

    [HttpGet("user/{userId?}")]
    public async Task<ActionResult> GetByUser(int? userId = null)
    {
        var currentUserId = userId ?? User.Identity.GetUserId();
        var patterns = await _filePatternService.GetFilePatternsByUserIdAsync(currentUserId);
        return Ok(patterns);
    }

    [HttpPut("{id}"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateFilePatternDto patternDto)
    {
        if (patternDto == null)
            throw new AppException("پترن نمی‌تواند خالی باشد");

        var userId = User.Identity.GetUserId();
        await _filePatternService.UpdateFilePatternAsync(id, patternDto, userId);
        return Ok(new { message = "پترن با موفقیت به‌روزرسانی شد" });
    }

    [HttpDelete("{id}"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = User.Identity.GetUserId();
        await _filePatternService.DeleteFilePatternAsync(id, userId);
        return Ok(new { message = "پترن با موفقیت حذف شد" });
    }

    [HttpPost("{id}/generate"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> GenerateFileName(int id, [FromBody] Dictionary<string, string> fieldValues)
    {
        var fileName = await _filePatternService.GenerateFileNameAsync(id, fieldValues);
        return Ok(new { fileName });
    }
}
