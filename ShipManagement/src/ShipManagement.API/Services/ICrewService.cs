using ShipManagement.API.Models;

namespace ShipManagement.API.Services
{
    public interface ICrewService
    {
        Task<PagedResult<CrewMember>> GetCrewListAsync(CrewListRequest request);
    }
}