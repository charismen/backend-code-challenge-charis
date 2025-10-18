using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;
using ShipManagement.API.Services;

namespace ShipManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized access");
                return Unauthorized("Unauthorized access");
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
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized access");
                return Unauthorized("Unauthorized access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ship with code {Code}", code);
                return StatusCode(500, "An error occurred while retrieving the ship");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Ship>> CreateShip(Ship ship)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ship.Code) || string.IsNullOrWhiteSpace(ship.Name) ||
                    string.IsNullOrWhiteSpace(ship.FiscalYear))
                {
                    return BadRequest("Ship code, name, and fiscal year are required");
                }

                var created = await _shipService.CreateShipAsync(ship);
                return CreatedAtAction(nameof(GetShipByCode), new { code = created.Code }, created);
            }
            catch (ConflictException cf)
            {
                _logger.LogWarning(cf, "Ship creation failed for code {Code}: {Message}", ship.Code, cf.Message);
                return Conflict(cf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized access");
                return Unauthorized("Unauthorized access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ship");
                return StatusCode(500, "An error occurred while creating the ship");
            }
        }

        [HttpPut("{code}")]
        public async Task<IActionResult> UpdateShip(string code, Ship ship)
        {
            try
            {
                if (!string.Equals(code, ship.Code, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Ship code mismatch");
                if (string.IsNullOrWhiteSpace(ship.Name) || string.IsNullOrWhiteSpace(ship.FiscalYear))
                    return BadRequest("Ship name and fiscal year are required");

                ship.Name = ship.Name.Trim();
                ship.FiscalYear = ship.FiscalYear.Trim();
                var updated = await _shipService.UpdateShipAsync(ship);
                if (updated is null)
                    return NotFound($"Ship with code {code} not found");

                return NoContent();
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "Ship with code {Code} not found during update", code);
                return NotFound(nf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized access");
                return Unauthorized("Unauthorized access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ship with code {Code}", code);
                return StatusCode(500, "An error occurred while updating the ship");
            }
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> DeleteShip(string code)
        {
            try
            {
                var success = await _shipService.DeleteShipAsync(code);
                if (!success)
                    return NotFound($"Ship with code {code} not found");

                return NoContent();
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "Ship with code {Code} not found during delete", code);
                return NotFound(nf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized access");
                return Unauthorized("Unauthorized access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ship with code {Code}", code);
                return StatusCode(500, "An error occurred while deleting the ship");
            }
        }
    }
}
