using System;
using Microsoft.Data.SqlClient;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IDapperExecutor _dapper;

        public FinancialService(IDatabaseConnectionFactory connectionFactory, IDapperExecutor dapper)
        {
            _connectionFactory = connectionFactory;
            _dapper = dapper;
        }

        public async Task<IEnumerable<FinancialReportItem>> GetFinancialReportDetailAsync(FinancialReportRequest request)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var accountPeriod = new DateTime(request.Year, request.Month, 1);
                return await _dapper.QueryAsync<FinancialReportItem>(
                    connection,
                    "EXEC GetFinancialReportDetail @ShipCode, @AccountPeriod",
                    new { request.ShipCode, AccountPeriod = accountPeriod });
            }
            catch (SqlException ex) when (IsShipNotFound(ex))
            {
                throw new NotFoundException($"Ship with code {request.ShipCode} not found");
            }
        }

        public async Task<IEnumerable<FinancialReportItem>> GetFinancialReportSummaryAsync(FinancialReportRequest request)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var accountPeriod = new DateTime(request.Year, request.Month, 1);
                return await _dapper.QueryAsync<FinancialReportItem>(
                    connection,
                    "EXEC GetFinancialReportSummary @ShipCode, @AccountPeriod",
                    new { request.ShipCode, AccountPeriod = accountPeriod });
            }
            catch (SqlException ex) when (IsShipNotFound(ex))
            {
                throw new NotFoundException($"Ship with code {request.ShipCode} not found");
            }
        }

        private static bool IsShipNotFound(SqlException ex) =>
            ex.Message.IndexOf("Ship not found", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
