using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ShipManagement.API.Controllers;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;
using ShipManagement.API.Services;
using Xunit;

namespace ShipManagement.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task CreateUser_ReturnsCreated_WhenPayloadValid()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);
        var request = new CreateUserRequest { Name = "Alice", Role = "Admin" };
        var createdUser = new User { UserId = 42, Name = "Alice", Role = "Admin" };

        service.CreateUserAsync(Arg.Is<User>(u => u.UserId == 0 && u.Name == "Alice" && u.Role == "Admin"))
            .Returns(createdUser);

        var response = await controller.CreateUser(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var payload = Assert.IsType<User>(createdResult.Value);
        Assert.Equal(createdUser.UserId, payload.UserId);
        await service.Received(1).CreateUserAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenMandatoryFieldsMissing()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);
        var request = new CreateUserRequest { Name = "", Role = "" };

        var response = await controller.CreateUser(request);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        await service.DidNotReceiveWithAnyArgs().CreateUserAsync(default!);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsUnauthorized_WhenServiceThrowsUnauthorized()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);

        service.GetAllUsersAsync()
            .ThrowsAsync(new UnauthorizedAccessException());

        var response = await controller.GetAllUsers();

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        Assert.Equal("Unauthorized access", unauthorized.Value);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenServiceReturnsNull()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);
        var user = new User { UserId = 1, Name = "Alice", Role = "Admin" };

        service.UpdateUserAsync(user).Returns(Task.FromResult<User?>(null));

        var response = await controller.UpdateUser(1, user);

        Assert.IsType<NotFoundObjectResult>(response);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenServiceThrowsNotFound()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);
        var user = new User { UserId = 1, Name = "Alice", Role = "Admin" };

        service.UpdateUserAsync(Arg.Any<User>())
            .ThrowsAsync(new NotFoundException("User with ID 1 not found"));

        var response = await controller.UpdateUser(1, user);

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal("User with ID 1 not found", notFound.Value);
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequest_WhenMandatoryFieldsMissing()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);
        var user = new User { UserId = 1, Name = string.Empty, Role = " " };

        var response = await controller.UpdateUser(1, user);

        Assert.IsType<BadRequestObjectResult>(response);
        await service.DidNotReceiveWithAnyArgs().UpdateUserAsync(default!);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenServiceThrowsNotFound()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);

        service.DeleteUserAsync(1).ThrowsAsync(new NotFoundException("User with ID 1 not found"));

        var response = await controller.DeleteUser(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal("User with ID 1 not found", notFound.Value);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNoContent_WhenServiceSucceeds()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);

        var response = await controller.DeleteUser(1);

        Assert.IsType<NoContentResult>(response);
        await service.Received(1).DeleteUserAsync(1);
    }

    [Fact]
    public async Task AssignShip_ReturnsNotFound_WhenServiceThrowsNotFound()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);

        service.AssignShipToUserAsync(1, "SHIP01")
            .ThrowsAsync(new NotFoundException("Ship with code SHIP01 not found"));

        var response = await controller.AssignShipToUser(1, "SHIP01");

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal("Ship with code SHIP01 not found", notFound.Value);
    }

    [Fact]
    public async Task AssignShip_ReturnsConflict_WhenServiceThrowsConflict()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);

        service.AssignShipToUserAsync(1, "SHIP01")
            .ThrowsAsync(new ConflictException("Assignment between user 1 and ship SHIP01 already exists"));

        var response = await controller.AssignShipToUser(1, "SHIP01");

        var conflict = Assert.IsType<ConflictObjectResult>(response);
        Assert.Equal("Assignment between user 1 and ship SHIP01 already exists", conflict.Value);
    }

    [Fact]
    public async Task RemoveShip_ReturnsNotFound_WhenServiceThrowsNotFound()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);

        service.RemoveShipFromUserAsync(1, "SHIP01")
            .ThrowsAsync(new NotFoundException("Assignment between user 1 and ship SHIP01 not found"));

        var response = await controller.RemoveShipFromUser(1, "SHIP01");

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal("Assignment between user 1 and ship SHIP01 not found", notFound.Value);
    }

    [Fact]
    public async Task GetShipsByUser_ReturnsNotFound_WhenServiceThrowsNotFound()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);

        service.GetShipsByUserAsync(1)
            .ThrowsAsync(new NotFoundException("User with ID 1 not found"));

        var response = await controller.GetShipsByUser(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("User with ID 1 not found", notFound.Value);
    }

    [Fact]
    public async Task GetShipsByUser_ReturnsNotFound_WhenUserHasNoAssignments()
    {
        var service = Substitute.For<IUserService>();
        var logger = Substitute.For<ILogger<UsersController>>();
        var controller = new UsersController(service, logger);

        service.GetShipsByUserAsync(1)
            .ThrowsAsync(new NotFoundException("User with ID 1 is not assigned to any ships"));

        var response = await controller.GetShipsByUser(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("User with ID 1 is not assigned to any ships", notFound.Value);
    }
}
