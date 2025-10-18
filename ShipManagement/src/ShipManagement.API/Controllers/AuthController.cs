using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShipManagement.API.Auth;
using ShipManagement.API.Models;

namespace ShipManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;
        private readonly AuthUserOptions _userOptions;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IJwtTokenService tokenService,
            IOptions<AuthUserOptions> userOptions,
            ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _userOptions = userOptions.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<LoginResponse> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required");

            var user = _userOptions.Users
                .FirstOrDefault(u =>
                    string.Equals(u.Username, request.Username, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == request.Password);

            if (user is null)
            {
                _logger.LogWarning("Failed login attempt for username {Username}", request.Username);
                return Unauthorized("Invalid credentials");
            }

            var token = _tokenService.GenerateToken(user.Username, user.Role);
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            return Ok(new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                ExpiresAtUtc = expiresAt
            });
        }
    }
}
