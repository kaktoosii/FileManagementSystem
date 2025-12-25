using Azure.Core;
using Base.Common.Features.Identity;
using Base.DomainClasses;
using Base.Services;
using Base.ViewModels;
using Base.ViewModels.Dto.Support;
using Base.ViewModels.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ViewModels.Dto;

namespace Base.WebApp.Controllers;

[Route("api/v1/[controller]"), EnableCors("CorsPolicy"), Authorize(Policy = CustomPolicies.DynamicServerPermission)]
[DisplayName("پشتیبانی")]
public class SupportsController : Controller
{
    private readonly ISupportService _supportService;

    public SupportsController(ISupportService supportService)
    {
        _supportService = supportService;
    }


    /// <summary>
    /// Admin responds to a support request
    /// </summary>
    [HttpPost("respond/{requestId}"), IgnoreAntiforgeryToken]
    [Authorize(Roles = "Admin")] // Only Admin can respond
    public async Task<IActionResult> RespondToSupportRequest(int requestId, [FromBody] SupportResponseDto responseDto)
    {
        try
        {
            await _supportService.RespondToSupportRequestAsync(requestId, responseDto);
            return Ok(new { Message = "Response sent successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get all unresolved support requests
    /// </summary>
    [HttpGet()]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResponse<List<SupportRequestViewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSupportRequests(SupportFilterDto filterDto)
    {
        var requests = await _supportService.GetSupportRequestsAsync(filterDto);
        return Ok(requests);
    }

    /// <summary>
    /// Get details of a support request including response
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Response<SupportRequestDetailViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportRequestById(int id)
    {
        try
        {
            var request = await _supportService.GetSupportRequestByIdAsync(id);
            return Ok(new Response<SupportRequestDetailViewModel>
            {
                Data = request,
                Succeeded = true
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

   
}
