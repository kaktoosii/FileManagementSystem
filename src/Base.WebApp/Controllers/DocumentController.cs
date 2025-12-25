using Base.Common.Helpers;
using Base.Services;
using Base.ViewModels.Dto;
using Common.IdentityToolkit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using System;
using System.Net;
using System.Threading.Tasks;
using ViewModels.Dto;

namespace Areas.Identity.Controllers;
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v1/[controller]"), EnableCors("CorsPolicy"), Authorize()]
public class DocumentController : Controller
{
    private readonly IDocumentService _documentServices;

    public DocumentController(
        IDocumentService documentServices)
    {
        _documentServices = documentServices;
    }


    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<ActionResult> Upload(IFormFile document)
    {
        if (document == null)
            throw new AppException(nameof(document));
        var documentForUpload = new DocumentDto(document);
        var uploaderIp = this.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString();
        documentForUpload.SetUploaderIp(uploaderIp);
        var userId = User.Identity.GetUserId();
        return this.Ok(await this._documentServices.UploadDocument(documentForUpload, userId, uploaderIp));
    }
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult> Download([FromQuery] string DocumentId)
    {
        if (string.IsNullOrWhiteSpace(DocumentId))
        {
            var brokenDocument = _documentServices.BrokenImage();
            return File(brokenDocument, "image/webp");
        }

        if (!Guid.TryParse(DocumentId, out Guid documentGuid))
        {
            return NotFound("فایلی یافت نشد");
        }

        var document = await _documentServices.DownloadDocument(documentGuid);

        if (document == null)
        {
            return NotFound("فایلی یافت نشد");
        }

        return File(document, "image/webp");
    }
}