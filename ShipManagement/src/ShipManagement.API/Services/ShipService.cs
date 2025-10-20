using System;
using Microsoft.Data.SqlClient;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public class ShipService : IShipService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IDapperExecutor _dapper;

        public ShipService(IDatabaseConnectionFactory connectionFactory, IDapperExecutor dapper)
        {
            _connectionFactory = connectionFactory;
            _dapper = dapper;
        }

        public async Task<IEnumerable<Ship>> GetAllShipsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await _dapper.QueryAsync<Ship>(connection, "EXEC GetShips");
        }

        public async Task<Ship?> GetShipByCodeAsync(string code)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await _dapper.QueryFirstOrDefaultAsync<Ship>(
                connection, "EXEC GetShipByCode @Code", new { Code = code });
        }

        public async Task<Ship> CreateShipAsync(Ship ship)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                return await _dapper.QuerySingleAsync<Ship>(
                    connection,
                    "EXEC CreateShip @Code, @Name, @FiscalYear, @Status",
                    new { ship.Code, ship.Name, ship.FiscalYear, ship.Status });
            }
            catch (SqlException ex) when (IsShipAlreadyExists(ex))
            {
                throw new ConflictException($"Ship with code {ship.Code} already exists");
            }
        }

        public async Task<Ship?> UpdateShipAsync(Ship ship)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                return await _dapper.QueryFirstOrDefaultAsync<Ship>(
                    connection,
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
                await _dapper.ExecuteAsync(connection, "EXEC DeleteShip @Code", new { Code = code });
                return true;
            }
            catch (SqlException ex) when (IsShipNotFound(ex))
            {
                throw new NotFoundException($"Ship with code {code} not found");
            }
        }

        private static bool IsShipNotFound(SqlException ex) =>
            ex.Message.IndexOf("Ship not found", StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool IsShipAlreadyExists(SqlException ex) =>
            ex.Message.IndexOf("Ship with this code already exists", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
