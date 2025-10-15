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
}
