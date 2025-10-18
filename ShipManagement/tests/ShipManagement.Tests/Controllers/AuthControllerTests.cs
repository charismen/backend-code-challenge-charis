using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ShipManagement.API.Auth;
using ShipManagement.API.Controllers;
using ShipManagement.API.Models;
using Xunit;

namespace ShipManagement.Tests.Controllers;

public class AuthControllerTests
{
    private static IOptions<AuthUserOptions> BuildUserOptions() =>
        Options.Create(new AuthUserOptions
        {
            Users = new List<AuthUser>
            {
                new() { Username = "admin", Password = "Password123!", Role = "Administrator" }
            }
        });

    private static IOptions<JwtOptions> BuildJwtOptions() =>
        Options.Create(new JwtOptions
        {
            Issuer = "ShipManagement",
            Audience = "ShipManagement",
            SigningKey = new string('k', 32),
            ExpiryMinutes = 60
        });

    [Fact]
    public void Login_ReturnsBadRequest_WhenCredentialsMissing()
    {
        var tokenService = Substitute.For<IJwtTokenService>();
        var controller = new AuthController(tokenService, BuildUserOptions(), Substitute.For<ILogger<AuthController>>());

        var response = controller.Login(new LoginRequest { Username = "", Password = "" });

        Assert.IsType<BadRequestObjectResult>(response.Result);
        tokenService.DidNotReceiveWithAnyArgs().GenerateToken(default!, default!);
    }

    [Fact]
    public void Login_ReturnsUnauthorized_WhenCredentialsInvalid()
    {
        var tokenService = Substitute.For<IJwtTokenService>();
        var controller = new AuthController(tokenService, BuildUserOptions(), Substitute.For<ILogger<AuthController>>());

        var response = controller.Login(new LoginRequest { Username = "admin", Password = "wrong" });

        Assert.IsType<UnauthorizedObjectResult>(response.Result);
        tokenService.DidNotReceiveWithAnyArgs().GenerateToken(default!, default!);
    }

    [Fact]
    public void Login_ReturnsToken_WhenCredentialsValid()
    {
        var jwtOptions = BuildJwtOptions();
        var tokenService = new JwtTokenService(jwtOptions);
        var controller = new AuthController(tokenService, BuildUserOptions(), Substitute.For<ILogger<AuthController>>());

        var response = controller.Login(new LoginRequest { Username = "admin", Password = "Password123!" });

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(payload.Token));
        Assert.Equal("admin", payload.Username);
        Assert.Equal("Administrator", payload.Role);
    }
}
