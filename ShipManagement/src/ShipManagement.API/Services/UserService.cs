using Dapper;
using ShipManagement.API.Data;
using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public class UserService : IUserService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public UserService(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<User>("EXEC GetUsers");
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "EXEC GetUserById @Id", new { Id = id });
        }

        public async Task<int> CreateUserAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(
                "EXEC CreateUser @Username, @Email, @FirstName, @LastName, @IsActive; SELECT SCOPE_IDENTITY();",
                new { user.Username, user.Email, user.FirstName, user.LastName, user.IsActive });
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "EXEC UpdateUser @Id, @Username, @Email, @FirstName, @LastName, @IsActive",
                new { user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.IsActive });
            return affectedRows > 0;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var affectedRows = await connection.ExecuteAsync("EXEC DeleteUser @Id", new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> AssignShipToUserAsync(int userId, int shipId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "EXEC AssignShipToUser @UserId, @ShipId", 
                new { UserId = userId, ShipId = shipId });
            return affectedRows > 0;
        }

        public async Task<bool> RemoveShipFromUserAsync(int userId, int shipId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "EXEC RemoveShipFromUser @UserId, @ShipId", 
                new { UserId = userId, ShipId = shipId });
            return affectedRows > 0;
        }

        public async Task<IEnumerable<Ship>> GetShipsByUserAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Ship>(
                "EXEC GetShipsByUser @UserId", new { UserId = userId });
        }
    }
}