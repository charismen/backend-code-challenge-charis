using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int userId);
        Task<User> CreateUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> AssignShipToUserAsync(int userId, string shipCode);
        Task<bool> RemoveShipFromUserAsync(int userId, string shipCode);
        Task<IEnumerable<Ship>> GetShipsByUserAsync(int userId);
    }
}
