using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ShipManagement.API.Controllers;
using ShipManagement.API.Models;
using ShipManagement.API.Services;
using Xunit;

namespace ShipManagement.Tests.Controllers;

public class ShipsControllerTests
{
    [Fact]
    public async Task GetAllShips_ReturnsOk_WhenServiceReturnsShips()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);
        var ships = new List<Ship>
        {
            new() { Id = 1, Code = "SHIP1", Name = "Ship 1", YearBuilt = 2020 },
            new() { Id = 2, Code = "SHIP2", Name = "Ship 2", YearBuilt = 2021 }
        }.AsEnumerable();

        service.GetAllShipsAsync().Returns(Task.FromResult(ships));

        var response = await controller.GetAllShips();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsAssignableFrom<IEnumerable<Ship>>(okResult.Value);
        Assert.Equal(2, payload.Count());
    }

    [Fact]
    public async Task GetAllShips_ReturnsInternalServerError_WhenServiceThrows()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);

        service.GetAllShipsAsync().ThrowsAsync(new InvalidOperationException("failure"));

        var response = await controller.GetAllShips();

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetShipByCode_ReturnsOk_WhenShipExists()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);
        var ship = new Ship { Id = 1, Code = "SHIP1", Name = "Ship 1", YearBuilt = 2020 };

        service.GetShipByCodeAsync("SHIP1").Returns(Task.FromResult<Ship?>(ship));

        var response = await controller.GetShipByCode("SHIP1");

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<Ship>(okResult.Value);
        Assert.Equal("SHIP1", payload.Code);
    }

    [Fact]
    public async Task GetShipByCode_ReturnsNotFound_WhenShipMissing()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);

        service.GetShipByCodeAsync("MISSING").Returns(Task.FromResult<Ship?>(null));

        var response = await controller.GetShipByCode("MISSING");

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetShipByCode_ReturnsInternalServerError_WhenServiceThrows()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);

        service.GetShipByCodeAsync(Arg.Any<string>()).ThrowsAsync(new InvalidOperationException("failure"));

        var response = await controller.GetShipByCode("SHIP1");

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateShip_ReturnsCreated_WhenPayloadValid()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);
        var ship = new Ship { Code = "SHIP3", Name = "Ship 3", YearBuilt = 2022 };

        service.CreateShipAsync(ship).Returns(Task.FromResult(3));

        var response = await controller.CreateShip(ship);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var payload = Assert.IsType<int>(createdResult.Value);
        Assert.Equal(3, payload);
        Assert.Equal(nameof(ShipsController.GetShipByCode), createdResult.ActionName);
    }

    [Fact]
    public async Task CreateShip_ReturnsBadRequest_WhenMandatoryFieldsMissing()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);
        var ship = new Ship { Code = string.Empty, Name = string.Empty, YearBuilt = 2022 };

        var response = await controller.CreateShip(ship);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        await service.DidNotReceiveWithAnyArgs().CreateShipAsync(default!);
    }

    [Fact]
    public async Task CreateShip_ReturnsInternalServerError_WhenServiceThrows()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);
        var ship = new Ship { Code = "SHIP4", Name = "Ship 4", YearBuilt = 2022 };

        service.CreateShipAsync(Arg.Any<Ship>()).ThrowsAsync(new InvalidOperationException("failure"));

        var response = await controller.CreateShip(ship);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
