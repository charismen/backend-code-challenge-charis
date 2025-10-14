using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<int>> CreateUser(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
                    return BadRequest("Username and email are required");

                var id = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { id }, id);
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
                if (id != user.Id)
                    return BadRequest("User ID mismatch");

                var success = await _userService.UpdateUserAsync(user);
                if (!success)
                    return NotFound($"User with ID {id} not found");

                return NoContent();
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
                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                    return NotFound($"User with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }

        [HttpPost("{userId}/ships/{shipId}")]
        public async Task<IActionResult> AssignShipToUser(int userId, int shipId)
        {
            try
            {
                var success = await _userService.AssignShipToUserAsync(userId, shipId);
                if (!success)
                    return NotFound("User or ship not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ship {ShipId} to user {UserId}", shipId, userId);
                return StatusCode(500, "An error occurred while assigning the ship to the user");
            }
        }

        [HttpDelete("{userId}/ships/{shipId}")]
        public async Task<IActionResult> RemoveShipFromUser(int userId, int shipId)
        {
            try
            {
                var success = await _userService.RemoveShipFromUserAsync(userId, shipId);
                if (!success)
                    return NotFound("User-ship assignment not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing ship {ShipId} from user {UserId}", shipId, userId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ships for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving ships for the user");
            }
        }
    }
}