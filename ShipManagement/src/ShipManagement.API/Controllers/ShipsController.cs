using Microsoft.AspNetCore.Mvc;
using ShipManagement.API.Models;
using ShipManagement.API.Services;

namespace ShipManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipsController : ControllerBase
    {
        private readonly IShipService _shipService;
        private readonly ILogger<ShipsController> _logger;

        public ShipsController(IShipService shipService, ILogger<ShipsController> logger)
        {
            _shipService = shipService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ship>>> GetAllShips()
        {
            try
            {
                var ships = await _shipService.GetAllShipsAsync();
                return Ok(ships);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ships");
                return StatusCode(500, "An error occurred while retrieving ships");
            }
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<Ship>> GetShipByCode(string code)
        {
            try
            {
                var ship = await _shipService.GetShipByCodeAsync(code);
                if (ship == null)
                    return NotFound($"Ship with code {code} not found");

                return Ok(ship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ship with code {Code}", code);
                return StatusCode(500, "An error occurred while retrieving the ship");
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateShip(Ship ship)
        {
            try
            {
                if (string.IsNullOrEmpty(ship.Code) || string.IsNullOrEmpty(ship.Name))
                    return BadRequest("Ship code and name are required");

                var id = await _shipService.CreateShipAsync(ship);
                return CreatedAtAction(nameof(GetShipByCode), new { code = ship.Code }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ship");
                return StatusCode(500, "An error occurred while creating the ship");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShip(int id, Ship ship)
        {
            try
            {
                if (id != ship.Id)
                    return BadRequest("Ship ID mismatch");

                var success = await _shipService.UpdateShipAsync(ship);
                if (!success)
                    return NotFound($"Ship with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ship with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the ship");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShip(int id)
        {
            try
            {
                var success = await _shipService.DeleteShipAsync(id);
                if (!success)
                    return NotFound($"Ship with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ship with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the ship");
            }
        }
    }
}