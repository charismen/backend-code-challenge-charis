using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public interface IShipService
    {
        Task<IEnumerable<Ship>> GetAllShipsAsync();
        Task<Ship?> GetShipByCodeAsync(string code);
        Task<Ship> CreateShipAsync(Ship ship);
        Task<Ship?> UpdateShipAsync(Ship ship);
        Task<bool> DeleteShipAsync(string code);
    }
}
