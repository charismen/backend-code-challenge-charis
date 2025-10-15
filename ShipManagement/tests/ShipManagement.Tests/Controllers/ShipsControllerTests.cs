using System;
using System.Collections.Generic;
using System.Linq;
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
            new() { Code = "SHIP1", Name = "Ship 1", FiscalYear = "0112", Status = "Active" },
            new() { Code = "SHIP2", Name = "Ship 2", FiscalYear = "0212", Status = "Inactive" }
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
        var ship = new Ship { Code = "SHIP1", Name = "Ship 1", FiscalYear = "0112", Status = "Active" };

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
        var ship = new Ship { Code = "SHIP3", Name = "Ship 3", FiscalYear = "0112", Status = "Active" };
        var created = new Ship { Code = "SHIP3", Name = "Ship 3", FiscalYear = "0112", Status = "Active" };

        service.CreateShipAsync(ship).Returns(Task.FromResult(created));

        var response = await controller.CreateShip(ship);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var payload = Assert.IsType<Ship>(createdResult.Value);
        Assert.Equal(created.Code, payload.Code);
        Assert.Equal(nameof(ShipsController.GetShipByCode), createdResult.ActionName);
    }

    [Fact]
    public async Task CreateShip_ReturnsBadRequest_WhenMandatoryFieldsMissing()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);
        var ship = new Ship { Code = string.Empty, Name = string.Empty, FiscalYear = string.Empty, Status = string.Empty };

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
        var ship = new Ship { Code = "SHIP4", Name = "Ship 4", FiscalYear = "0112", Status = "Active" };

        service.CreateShipAsync(Arg.Any<Ship>()).ThrowsAsync(new InvalidOperationException("failure"));

        var response = await controller.CreateShip(ship);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateShip_ReturnsNotFound_WhenServiceSignalsMissing()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);
        var ship = new Ship { Code = "SHIP1", Name = "Ship 1", FiscalYear = "0112", Status = "Active" };

        service.UpdateShipAsync(ship).Returns(Task.FromResult<Ship?>(null));

        var response = await controller.UpdateShip("SHIP1", ship);

        Assert.IsType<NotFoundObjectResult>(response);
    }

    [Fact]
    public async Task UpdateShip_ReturnsNotFound_WhenServiceThrowsNotFound()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);
        var ship = new Ship { Code = "SHIP1", Name = "Ship 1", FiscalYear = "0112", Status = "Active" };

        service.UpdateShipAsync(Arg.Any<Ship>())
            .ThrowsAsync(new NotFoundException("Ship with code SHIP1 not found"));

        var response = await controller.UpdateShip("SHIP1", ship);

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal("Ship with code SHIP1 not found", notFound.Value);
    }

    [Fact]
    public async Task DeleteShip_ReturnsNotFound_WhenServiceThrowsNotFound()
    {
        var service = Substitute.For<IShipService>();
        var logger = Substitute.For<ILogger<ShipsController>>();
        var controller = new ShipsController(service, logger);

        service.DeleteShipAsync("SHIP1").ThrowsAsync(new NotFoundException("Ship with code SHIP1 not found"));

        var response = await controller.DeleteShip("SHIP1");

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal("Ship with code SHIP1 not found", notFound.Value);
    }
}
