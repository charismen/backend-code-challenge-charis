using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public interface IFinancialService
    {
        Task<IEnumerable<FinancialReportItem>> GetFinancialReportDetailAsync(FinancialReportRequest request);
        Task<IEnumerable<FinancialReportItem>> GetFinancialReportSummaryAsync(FinancialReportRequest request);
    }
}