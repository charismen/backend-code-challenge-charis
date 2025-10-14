using Dapper;
using ShipManagement.API.Data;
using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public FinancialService(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<FinancialReportItem>> GetFinancialReportDetailAsync(FinancialReportRequest request)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<FinancialReportItem>(
                "EXEC GetFinancialReportDetail @ShipId, @Year, @Month",
                new { request.ShipId, request.Year, request.Month });
        }

        public async Task<IEnumerable<FinancialReportItem>> GetFinancialReportSummaryAsync(FinancialReportRequest request)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<FinancialReportItem>(
                "EXEC GetFinancialReportSummary @ShipId, @Year, @Month",
                new { request.ShipId, request.Year, request.Month });
        }
    }
}