using Dapper;
using ShipManagement.API.Data;
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

        public async Task<int> CreateShipAsync(Ship ship)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(
                "EXEC CreateShip @Code, @Name, @YearBuilt; SELECT SCOPE_IDENTITY();", 
                new { ship.Code, ship.Name, ship.YearBuilt });
        }

        public async Task<bool> UpdateShipAsync(Ship ship)
        {
            using var connection = _connectionFactory.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "EXEC UpdateShip @Id, @Code, @Name, @YearBuilt", 
                new { ship.Id, ship.Code, ship.Name, ship.YearBuilt });
            return affectedRows > 0;
        }

        public async Task<bool> DeleteShipAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var affectedRows = await connection.ExecuteAsync("EXEC DeleteShip @Id", new { Id = id });
            return affectedRows > 0;
        }
    }
}