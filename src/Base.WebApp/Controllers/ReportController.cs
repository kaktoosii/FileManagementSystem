using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using System.Threading.Tasks;
using ViewModels;
using System;
using System.Collections.Generic;
using ViewModels.Dto;
using Base.Common.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using System.ComponentModel;

namespace WebAPI.Controllers;
[DisplayName("مدیریت گزارشات چاپی")]
[Route("api/v1/[controller]"), EnableCors("CorsPolicy"), Authorize(Policy = CustomPolicies.DynamicServerPermission)]
public class ReportController : Controller
{
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        }
        [DisplayName("ایجاد گزارشات چاپی")]
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateReport([FromBody] ReportViewModel report)
        {
            try
            {
                var reportId = await _reportService.AddNewReportAsync(report);
                return CreatedAtAction(nameof(GetReportById), new { reportId = reportId }, new { Id = reportId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [DisplayName("دریافت گزارشات چاپی")]
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReportById(int reportId)
        {
            try
            {
                var report = await _reportService.FindReportByIdAsync(reportId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [DisplayName("دریافت با کد گزارشات چاپی")]
        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetReportByCode(string code)
        {
            try
            {
                var report = await _reportService.FindReportByCodeAsync(code);
                if (report == null)
                    return NotFound(new { message = "Report not found." });
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [DisplayName("ویرایش گزارشات چاپی")]
        [HttpPut, IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateReport([FromBody] ReportViewModel report)
        {
            try
            {
                await _reportService.UpdateReportAsync(report);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [DisplayName("ویرایش متن گزارشات چاپی")]
        [HttpPut("body"), IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateReportBody([FromBody] ReportViewModel report)
        {
            try
            {
                await _reportService.UpdateReportBodyAsync(report);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [DisplayName("حذف گزارشات چاپی")]
        [HttpDelete("{reportId}"), IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            try
            {
                await _reportService.DeleteReportAsync(reportId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [DisplayName("دریافت همه گزارشات چاپی")]
        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            try
            {
                var reports = await _reportService.GetReportListAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }