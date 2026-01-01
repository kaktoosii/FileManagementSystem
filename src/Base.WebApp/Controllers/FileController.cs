using Base.Common.Helpers;
using Common.IdentityToolkit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ViewModels.Dto;

namespace Areas.Identity.Controllers;

[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v1/[controller]"), EnableCors("CorsPolicy"), Authorize()]
public class FileController : Controller
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> Upload([FromForm] IFormFile file, [FromForm] int? folderId, [FromForm] int? filePatternId, [FromForm] string patternValuesJson)
    {
        if (file == null)
            throw new AppException("فایل نمی‌تواند خالی باشد");

        var userId = User.Identity.GetUserId();
        var uploaderIp = HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString() ?? "";

        var uploadDto = new UploadFileDto
        {
            File = file,
            FolderId = folderId,
            FilePatternId = filePatternId
        };

        if (filePatternId.HasValue && !string.IsNullOrWhiteSpace(patternValuesJson))
        {
            try
            {
                uploadDto.PatternValues = JsonSerializer.Deserialize<Dictionary<string, string>>(patternValuesJson);
            }
            catch (Exception ex)
            {
                throw new AppException($"خطا در پردازش مقادیر پترن: {ex.Message}");
            }
        }

        var fileId = await _fileService.UploadFileAsync(uploadDto, userId, uploaderIp);
        return Ok(new { fileId, message = "فایل با موفقیت آپلود شد" });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var file = await _fileService.GetFileByIdAsync(id);
        if (file == null)
            return NotFound("فایل یافت نشد");

        var userId = User.Identity.GetUserId();
        if (file.UserId != userId)
            return Forbid("شما دسترسی به این فایل ندارید");

        return Ok(file);
    }

    [HttpGet("folder/{folderId}")]
    public async Task<ActionResult> GetByFolder(int folderId)
    {
        var files = await _fileService.GetFilesByFolderIdAsync(folderId);
        return Ok(files);
    }

    [HttpGet("user/{userId?}")]
    public async Task<ActionResult> GetByUser(int? userId = null)
    {
        var currentUserId = userId ?? User.Identity.GetUserId();
        var files = await _fileService.GetFilesByUserIdAsync(currentUserId);
        return Ok(files);
    }

    [HttpGet("{id}/download")]
    public async Task<ActionResult> Download(int id)
    {
        try
        {
            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null)
                return NotFound("فایل یافت نشد");

            var userId = User.Identity.GetUserId();
            if (file.UserId != userId)
                return Forbid("شما دسترسی به این فایل ندارید");

            var fileStream = await _fileService.DownloadFileAsync(id);
            return File(fileStream, file.MimeType, file.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("فایل یافت نشد");
        }
    }

    [HttpDelete("{id}"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = User.Identity.GetUserId();
        await _fileService.DeleteFileAsync(id, userId);
        return Ok(new { message = "فایل با موفقیت حذف شد" });
    }

    [HttpPatch("{id}/move"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> Move(int id, [FromBody] int? newFolderId)
    {
        var userId = User.Identity.GetUserId();
        await _fileService.MoveFileAsync(id, newFolderId, userId);
        return Ok(new { message = "فایل با موفقیت منتقل شد" });
    }
}
