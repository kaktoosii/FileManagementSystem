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
public class FolderController : Controller
{
    private readonly IFolderService _folderService;

    public FolderController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<ActionResult> Create([FromBody] CreateFolderDto folderDto)
    {
        if (folderDto == null)
            throw new AppException("فولدر نمی‌تواند خالی باشد");

        var userId = User.Identity.GetUserId();
        var folderId = await _folderService.CreateFolderAsync(folderDto, userId);
        return Ok(new { message = "فولدر با موفقیت ایجاد شد", folderId });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var folder = await _folderService.GetFolderByIdAsync(id);
        if (folder == null)
            return NotFound("فولدر یافت نشد");

        var userId = User.Identity.GetUserId();
        if (folder.UserId != userId)
            return Forbid("شما دسترسی به این فولدر ندارید");

        return Ok(folder);
    }

    [HttpGet("user/{userId?}")]
    public async Task<ActionResult> GetByUser(int? userId = null)
    {
        var currentUserId = userId ?? User.Identity.GetUserId();
        var folders = await _folderService.GetFoldersByUserIdAsync(currentUserId);
        return Ok(folders);
    }

    [HttpGet("root")]
    public async Task<ActionResult> GetRootFolders()
    {
        var userId = User.Identity.GetUserId();
        var folders = await _folderService.GetRootFoldersByUserIdAsync(userId);
        return Ok(folders);
    }

    [HttpGet("parent/{parentId}/children")]
    public async Task<ActionResult> GetSubFolders(int parentId)
    {
        var folders = await _folderService.GetSubFoldersAsync(parentId);
        return Ok(folders);
    }

    [HttpPut("{id}"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateFolderDto folderDto)
    {
        if (folderDto == null)
            throw new AppException("فولدر نمی‌تواند خالی باشد");

        var userId = User.Identity.GetUserId();
        await _folderService.UpdateFolderAsync(id, folderDto, userId);
        return Ok(new { message = "فولدر با موفقیت به‌روزرسانی شد" });
    }

    [HttpDelete("{id}"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = User.Identity.GetUserId();
        await _folderService.DeleteFolderAsync(id, userId);
        return Ok(new { message = "فولدر با موفقیت حذف شد" });
    }

    [HttpPatch("{id}/move"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> Move(int id, [FromBody] int? newParentFolderId)
    {
        var userId = User.Identity.GetUserId();
        await _folderService.MoveFolderAsync(id, newParentFolderId, userId);
        return Ok(new { message = "فولدر با موفقیت منتقل شد" });
    }

    [HttpGet("{id}/path"), IgnoreAntiforgeryToken]
    public async Task<ActionResult> GetPath(int id)
    {
        var path = await _folderService.GetFolderPathAsync(id);
        return Ok(new { path });
    }
}
