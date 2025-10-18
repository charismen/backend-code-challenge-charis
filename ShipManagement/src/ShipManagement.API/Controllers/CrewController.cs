using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipManagement.API.Models;
using ShipManagement.API.Services;

namespace ShipManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
                if (string.IsNullOrWhiteSpace(request.ShipCode))
                    return BadRequest("Ship code is required");
                if (request.PageNumber <= 0)
                    return BadRequest("Page number must be greater than zero");
                if (request.PageSize <= 0)
                    return BadRequest("Page size must be greater than zero");

                var result = await _crewService.GetCrewListAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized access");
                return Unauthorized("Unauthorized access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving crew list for ship {ShipCode}", request.ShipCode);
                return StatusCode(500, "An error occurred while retrieving the crew list");
            }
        }
    }
}
