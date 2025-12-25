using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Common.GuardToolkit;
using Common.IdentityToolkit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ViewModels;
using ViewModels.Dto;

namespace Services.Services;


public class ReportService : IReportService
{
    private readonly DbSet<Report> _Report;
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _contextAccessor;
    public ReportService(
        IUnitOfWork uow,
        IHttpContextAccessor contextAccessor,
        IDocumentService documentService)
    {
        _uow = uow;
        _uow.CheckArgumentIsNull(nameof(_uow));
        _Report = _uow.Set<Report>();
        _contextAccessor = contextAccessor;
        _contextAccessor.CheckArgumentIsNull(nameof(_contextAccessor));
    }

    public async Task<int> AddNewReportAsync(ReportViewModel Report)
    {
        if(Report == null)
            throw new AppException("یافت نشد.");
        var userId = _contextAccessor.HttpContext.User.Identity.GetUserId();
        var model = new Report(Report.Code, Report.Title, Report.ReportJson, userId);
        await _Report.AddAsync(model);
        await _uow.SaveChangesAsync();
        return model.Id;
    }


    public async Task DeleteReportAsync(int id)
    {
        var Report = await _Report.FindAsync(id);
        _Report.Remove(Report);
        await _uow.SaveChangesAsync();
    }

    public async Task<ReportViewModel> FindReportByIdAsync(int ReportId)
    {
        var report = await _Report.FindAsync(ReportId);
        if (report == null)
        {
            throw new AppException("یافت نشد.");
        }

        return new ReportViewModel()
        {
            Id = report.Id,
            Title = report.Title,
            Code = report.Code,
            ReportJson = report.ReportJson
        };
    }
    public async Task<ReportViewModel> FindReportByCodeAsync(string code)
    {
        var Report = await _Report.Include(x => x.User).Select(report => new ReportViewModel()
        {
            Id = report.Id,
            Title = report.Title,
            Code = report.Code,
            ReportJson = report.ReportJson
        }).FirstOrDefaultAsync(x => x.Code == code);

        return Report;
    }
    
    public async Task UpdateReportAsync(ReportViewModel Report)
    {
        if (Report == null)
            throw new AppException("یافت نشد.");
        var findedReport = await _Report.FindAsync(Report.Id);
        if (findedReport == null)
        {
            throw new AppException("یافت نشد.");
        }
        findedReport.Update(Report.Code, Report.Title, findedReport.ReportJson);
        _Report.Update(findedReport);
        await _uow.SaveChangesAsync();
    }
    public async Task UpdateReportBodyAsync(ReportViewModel report)
    {
        if (report == null)
            throw new AppException("یافت نشد.");
        var findedReport = await _Report.FindAsync(report.Id);
        if (findedReport == null)
        {
            throw new AppException("یافت نشد.");
        }
        findedReport.Update(findedReport.Code, findedReport.Title, report.ReportJson);
        _Report.Update(findedReport);
        await _uow.SaveChangesAsync();
    }
    public async Task<List<ReportViewModel>> GetReportListAsync()
    {
        var list = await _Report.Include(x => x.User).Select(report => new ReportViewModel
        {
            Id = report.Id,
            Title = report.Title,
            Code = report.Code
        }).ToListAsync();
        return list;
    }
}
