using System;
using Dapper;
using Microsoft.Data.SqlClient;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public class ShipService : IShipService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public ShipService(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Ship>> GetAllShipsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Ship>("EXEC GetShips");
        }

        public async Task<Ship?> GetShipByCodeAsync(string code)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Ship>(
                "EXEC GetShipByCode @Code", new { Code = code });
        }

        public async Task<Ship> CreateShipAsync(Ship ship)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<Ship>(
                "EXEC CreateShip @Code, @Name, @FiscalYear, @Status", 
                new { ship.Code, ship.Name, ship.FiscalYear, ship.Status });
        }

        public async Task<Ship?> UpdateShipAsync(Ship ship)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<Ship>(
                    "EXEC UpdateShip @Code, @Name, @FiscalYear, @Status", 
                    new { ship.Code, ship.Name, ship.FiscalYear, ship.Status });
            }
            catch (SqlException ex) when (IsShipNotFound(ex))
            {
                throw new NotFoundException($"Ship with code {ship.Code} not found");
            }
        }

        public async Task<bool> DeleteShipAsync(string code)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var affectedRows = await connection.ExecuteAsync("EXEC DeleteShip @Code", new { Code = code });
                return affectedRows > 0;
            }
            catch (SqlException ex) when (IsShipNotFound(ex))
            {
                throw new NotFoundException($"Ship with code {code} not found");
            }
        }

        private static bool IsShipNotFound(SqlException ex) =>
            ex.Message.IndexOf("Ship not found", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
