using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public class UserService : IUserService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IDapperExecutor _dapper;

        public UserService(IDatabaseConnectionFactory connectionFactory, IDapperExecutor dapper)
        {
            _connectionFactory = connectionFactory;
            _dapper = dapper;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await _dapper.QueryAsync<User>(connection, "EXEC GetUsers");
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await _dapper.QueryFirstOrDefaultAsync<User>(
                connection, "EXEC GetUserById @UserId", new { UserId = userId });
        }

        public async Task<User> CreateUserAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await _dapper.QuerySingleAsync<User>(
                connection,
                "EXEC CreateUser @Name, @Role",
                new { user.Name, user.Role });
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                return await _dapper.QueryFirstOrDefaultAsync<User>(
                    connection,
                    "EXEC UpdateUser @UserId, @Name, @Role",
                    new { UserId = user.UserId, user.Name, user.Role });
            }
            catch (SqlException ex) when (IsUserNotFound(ex))
            {
                throw new NotFoundException($"User with ID {user.UserId} not found");
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                await _dapper.ExecuteAsync(connection, "EXEC DeleteUser @UserId", new { UserId = userId });
            }
            catch (SqlException ex) when (IsUserNotFound(ex))
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }
        }

        public async Task<bool> AssignShipToUserAsync(int userId, string shipCode)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var assignment = await _dapper.QueryFirstOrDefaultAsync<dynamic>(
                    connection,
                    "EXEC AssignShipToUser @UserId, @ShipCode",
                    new { UserId = userId, ShipCode = shipCode });
                return assignment is not null;
            }
            catch (SqlException ex) when (IsUserNotFound(ex))
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }
            catch (SqlException ex) when (IsShipNotFound(ex))
            {
                throw new NotFoundException($"Ship with code {shipCode} not found");
            }
            catch (SqlException ex) when (IsAssignmentAlreadyExists(ex))
            {
                throw new ConflictException($"Assignment between user {userId} and ship {shipCode} already exists");
            }
        }

        public async Task<bool> RemoveShipFromUserAsync(int userId, string shipCode)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                await _dapper.ExecuteAsync(
                    connection,
                    "EXEC RemoveShipFromUser @UserId, @ShipCode",
                    new { UserId = userId, ShipCode = shipCode });
                return true;
            }
            catch (SqlException ex) when (IsAssignmentNotFound(ex))
            {
                throw new NotFoundException($"Assignment between user {userId} and ship {shipCode} not found");
            }
            catch (SqlException ex) when (IsUserNotFound(ex))
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }
            catch (SqlException ex) when (IsShipNotFound(ex))
            {
                throw new NotFoundException($"Ship with code {shipCode} not found");
            }
        }

        public async Task<IEnumerable<Ship>> GetShipsByUserAsync(int userId)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var ships = await _dapper.QueryAsync<Ship>(
                    connection,
                    "EXEC GetShipsByUser @UserId", new { UserId = userId });
                if (!ships.Any())
                {
                    throw new NotFoundException($"User with ID {userId} is not assigned to any ships");
                }

                return ships;
            }
            catch (SqlException ex) when (IsUserNotFound(ex))
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }
        }

        private static bool IsUserNotFound(SqlException ex) =>
            ex.Message.IndexOf("User not found", StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool IsShipNotFound(SqlException ex) =>
            ex.Message.IndexOf("Ship not found", StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool IsAssignmentNotFound(SqlException ex) =>
            ex.Message.IndexOf("Assignment not found", StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool IsAssignmentAlreadyExists(SqlException ex) =>
            ex.Message.IndexOf("Assignment already exists", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
