using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public interface IShipService
    {
        Task<IEnumerable<Ship>> GetAllShipsAsync();
        Task<Ship?> GetShipByCodeAsync(string code);
        Task<int> CreateShipAsync(Ship ship);
        Task<bool> UpdateShipAsync(Ship ship);
        Task<bool> DeleteShipAsync(int id);
    }
}