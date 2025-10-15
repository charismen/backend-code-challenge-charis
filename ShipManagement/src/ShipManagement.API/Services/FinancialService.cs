using System;
using Dapper;
using Microsoft.Data.SqlClient;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
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
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var accountPeriod = new DateTime(request.Year, request.Month, 1);
                return await connection.QueryAsync<FinancialReportItem>(
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
                return await connection.QueryAsync<FinancialReportItem>(
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
