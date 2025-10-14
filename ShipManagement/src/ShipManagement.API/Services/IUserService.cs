using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<int> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AssignShipToUserAsync(int userId, int shipId);
        Task<bool> RemoveShipFromUserAsync(int userId, int shipId);
        Task<IEnumerable<Ship>> GetShipsByUserAsync(int userId);
    }
}