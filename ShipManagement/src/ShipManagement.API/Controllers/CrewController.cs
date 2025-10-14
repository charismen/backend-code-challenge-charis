using Microsoft.AspNetCore.Mvc;
using ShipManagement.API.Models;
using ShipManagement.API.Services;

namespace ShipManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrewController : ControllerBase
    {
        private readonly ICrewService _crewService;
        private readonly ILogger<CrewController> _logger;

        public CrewController(ICrewService crewService, ILogger<CrewController> logger)
        {
            _crewService = crewService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<CrewMember>>> GetCrewList([FromQuery] CrewListRequest request)
        {
            try
            {
                if (request.ShipId <= 0)
                    return BadRequest("Valid ShipId is required");

                var result = await _crewService.GetCrewListAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving crew list for ship {ShipId}", request.ShipId);
                return StatusCode(500, "An error occurred while retrieving the crew list");
            }
        }
    }
}