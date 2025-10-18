using System;
using Dapper;
using Microsoft.Data.SqlClient;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
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

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "EXEC GetUserById @UserId", new { UserId = userId });
        }

        public async Task<User> CreateUserAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<User>(
                "EXEC CreateUser @Name, @Role",
                new { user.Name, user.Role });
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<User>(
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
                await connection.ExecuteAsync("EXEC DeleteUser @UserId", new { UserId = userId });
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
                var assignment = await connection.QueryFirstOrDefaultAsync(
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
                await connection.ExecuteAsync(
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
                var ships = await connection.QueryAsync<Ship>(
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
