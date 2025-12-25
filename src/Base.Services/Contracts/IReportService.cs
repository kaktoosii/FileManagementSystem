using System.Collections.Generic;
using System.Threading.Tasks;
using ViewModels;
using ViewModels.Dto;

namespace Services.Contracts;

public interface IReportService
{
    Task<int> AddNewReportAsync(ReportViewModel Report);
    Task<List<ReportViewModel>> GetReportListAsync();
    Task DeleteReportAsync(int id);
    Task UpdateReportBodyAsync(ReportViewModel report);
    Task<ReportViewModel> FindReportByCodeAsync(string code);
    Task<ReportViewModel> FindReportByIdAsync(int ReportId);
    Task UpdateReportAsync(ReportViewModel Report);
}
