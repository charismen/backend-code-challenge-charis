using Microsoft.AspNetCore.Mvc;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;
using ShipManagement.API.Services;

namespace ShipManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }   

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound($"User with ID {id} not found");

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Role))
                    return BadRequest("User name and role are required");

                var newUser = new User
                {
                    Name = request.Name.Trim(),
                    Role = request.Role.Trim()
                };

                var created = await _userService.CreateUserAsync(newUser);
                return CreatedAtAction(nameof(GetUserById), new { id = created.UserId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "An error occurred while creating the user");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            try
            {
                if (id != user.UserId)
                    return BadRequest("User ID mismatch");

                var updated = await _userService.UpdateUserAsync(user);
                if (updated is null)
                    return NotFound($"User with ID {id} not found");

                return NoContent();
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "User with ID {Id} not found during update", id);
                return NotFound(nf.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "User with ID {Id} not found during delete", id);
                return NotFound(nf.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }

        [HttpPost("{userId}/ships/{shipCode}")]
        public async Task<IActionResult> AssignShipToUser(int userId, string shipCode)
        {
            try
            {
                var success = await _userService.AssignShipToUserAsync(userId, shipCode);
                if (!success)
                    return NotFound("User or ship not found");

                return NoContent();
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "Assign ship {ShipCode} to user {UserId} failed: {Message}", shipCode, userId, nf.Message);
                return NotFound(nf.Message);
            }
            catch (ConflictException cf)
            {
                _logger.LogWarning(cf, "Assign ship {ShipCode} to user {UserId} failed: {Message}", shipCode, userId, cf.Message);
                return Conflict(cf.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ship {ShipCode} to user {UserId}", shipCode, userId);
                return StatusCode(500, "An error occurred while assigning the ship to the user");
            }
        }

        [HttpDelete("{userId}/ships/{shipCode}")]
        public async Task<IActionResult> RemoveShipFromUser(int userId, string shipCode)
        {
            try
            {
                var success = await _userService.RemoveShipFromUserAsync(userId, shipCode);
                if (!success)
                    return NotFound("User-ship assignment not found");

                return NoContent();
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "Remove ship {ShipCode} from user {UserId} failed: {Message}", shipCode, userId, nf.Message);
                return NotFound(nf.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing ship {ShipCode} from user {UserId}", shipCode, userId);
                return StatusCode(500, "An error occurred while removing the ship from the user");
            }
        }

        [HttpGet("{userId}/ships")]
        public async Task<ActionResult<IEnumerable<Ship>>> GetShipsByUser(int userId)
        {
            try
            {
                var ships = await _userService.GetShipsByUserAsync(userId);
                return Ok(ships);
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "User with ID {UserId} not found when fetching ships", userId);
                return NotFound(nf.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ships for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving ships for the user");
            }
        }
    }
}
