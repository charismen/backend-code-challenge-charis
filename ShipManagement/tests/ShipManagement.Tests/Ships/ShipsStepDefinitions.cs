using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using ShipManagement.API.Controllers;
using ShipManagement.API.Models;
using ShipManagement.API.Services;
using TechTalk.SpecFlow;

namespace ShipManagement.Tests.Ships;

[Binding]
[Scope(Feature = "Ship management")]
public class ShipsStepDefinitions
{
    private readonly IShipService _shipService;
    private readonly ILogger<ShipsController> _logger;
    private readonly ShipsController _controller;

    private ActionResult<IEnumerable<Ship>>? _shipListResponse;
    private ActionResult<Ship>? _shipResponse;
    private ActionResult<int>? _createShipResponse;
    private IActionResult? _lastActionResult;
    private IEnumerable<Ship>? _actualShipList;
    private Ship? _actualShip;
    private int? _actualCreatedShipId;
    private Ship _shipUnderTest = null!;
    private int _configuredCreatedShipId = 3;

    public ShipsStepDefinitions()
    {
        _shipService = Substitute.For<IShipService>();
        _logger = Substitute.For<ILogger<ShipsController>>();
        _controller = new ShipsController(_shipService, _logger);
    }

    [Given(@"ships (.+) and (.+) exist")]
    public void GivenShipsAndExist(string firstCode, string secondCode)
    {
        var existingShips = new List<Ship>
        {
            new Ship { Id = 1, Code = firstCode, Name = $"{firstCode} Name", YearBuilt = 2020 },
            new Ship { Id = 2, Code = secondCode, Name = $"{secondCode} Name", YearBuilt = 2021 }
        };

        _shipService.GetAllShipsAsync()
            .Returns(Task.FromResult<IEnumerable<Ship>>(existingShips));
    }

    [Given(@"the ship service fails when retrieving ships")]
    public void GivenTheShipServiceFailsWhenRetrievingShips()
    {
        _shipService.GetAllShipsAsync()
            .ThrowsAsync(new InvalidOperationException("Ship service failure"));
    }

    [Given(@"ship (.+) exists")]
    public void GivenShipExists(string code)
    {
        var ship = new Ship { Id = 1, Code = code, Name = $"{code} Name", YearBuilt = 2020 };

        _shipService.GetShipByCodeAsync(code)
            .Returns(Task.FromResult<Ship?>(ship));
    }

    [Given(@"no ship exists with code (.+)")]
    public void GivenNoShipExistsWithCode(string code)
    {
        _shipService.GetShipByCodeAsync(code)
            .Returns(Task.FromResult<Ship?>(null));
    }

    [Given(@"a new ship with code (.+) and year built (\d{4})")]
    public void GivenANewShipWithCodeAndYearBuilt(string code, int yearBuilt)
    {
        _shipUnderTest = new Ship { Code = code, Name = $"{code} Name", YearBuilt = yearBuilt };

        _shipService.CreateShipAsync(Arg.Any<Ship>())
            .Returns(Task.FromResult(_configuredCreatedShipId));
    }

    [Given(@"a new ship without a code")]
    public void GivenANewShipWithoutACode()
    {
        _shipUnderTest = new Ship { Code = string.Empty, Name = string.Empty, YearBuilt = 2022 };
    }

    [When(@"all ships are requested")]
    public async Task WhenAllShipsAreRequested()
    {
        _shipListResponse = await _controller.GetAllShips();
        _lastActionResult = _shipListResponse.Result;
    }

    [When(@"ship (.+) is requested by code")]
    public async Task WhenShipIsRequestedByCode(string code)
    {
        _shipResponse = await _controller.GetShipByCode(code);
        _lastActionResult = _shipResponse.Result;
    }

    [When(@"the ship is created")]
    public async Task WhenTheShipIsCreated()
    {
        _createShipResponse = await _controller.CreateShip(_shipUnderTest);
        _lastActionResult = _createShipResponse.Result;
    }

    [Then(@"the response status should be (.+)")]
    public void ThenTheResponseStatusShouldBe(string expectedStatus)
    {
        Assert.That(_lastActionResult, Is.Not.Null, "No action result was captured");

        switch (expectedStatus)
        {
            case "OK":
                var okResult = _lastActionResult as OkObjectResult;
                Assert.That(okResult, Is.Not.Null, "Expected OK result but received a different response");

                switch (okResult!.Value)
                {
                    case IEnumerable<Ship> ships:
                        _actualShipList = ships;
                        _actualShip = null;
                        _actualCreatedShipId = null;
                        break;
                    case Ship ship:
                        _actualShip = ship;
                        _actualShipList = null;
                        _actualCreatedShipId = null;
                        break;
                    default:
                        Assert.Fail("OK response contained an unexpected payload type");
                        break;
                }
                break;

            case "NotFound":
                Assert.That(_lastActionResult, Is.TypeOf<NotFoundObjectResult>(),
                    "Expected NotFound result but received a different response");
                _actualShipList = null;
                _actualShip = null;
                _actualCreatedShipId = null;
                break;

            case "Created":
                var createdResult = _lastActionResult as CreatedAtActionResult;
                Assert.That(createdResult, Is.Not.Null, "Expected Created result but received a different response");

                if (createdResult!.Value is int createdId)
                {
                    _actualCreatedShipId = createdId;
                }
                else
                {
                    Assert.Fail("Created response did not contain an integer ship identifier");
                }

                _actualShipList = null;
                _actualShip = null;
                break;

            case "BadRequest":
                Assert.That(_lastActionResult, Is.TypeOf<BadRequestObjectResult>(),
                    "Expected BadRequest result but received a different response");
                _actualShipList = null;
                _actualShip = null;
                _actualCreatedShipId = null;
                break;

            case "InternalServerError":
                var objectResult = _lastActionResult as ObjectResult;
                Assert.That(objectResult, Is.Not.Null, "Expected ObjectResult for InternalServerError");
                Assert.That(objectResult!.StatusCode, Is.EqualTo(500), "Expected status code 500");
                _actualShipList = null;
                _actualShip = null;
                _actualCreatedShipId = null;
                break;

            default:
                Assert.Fail($"Unsupported status expectation: '{expectedStatus}'");
                break;
        }
    }

    [Then(@"the ship list should contain (\d+) ships")]
    public void ThenTheShipListShouldContainShips(int expectedCount)
    {
        Assert.That(_actualShipList, Is.Not.Null, "No ship list payload is available");
        Assert.That(_actualShipList!.Count(), Is.EqualTo(expectedCount), "Ship count mismatch");
    }

    [Then(@"the returned ship should have code (.+)")]
    public void ThenTheReturnedShipShouldHaveCode(string expectedCode)
    {
        Assert.That(_actualShip, Is.Not.Null, "No ship payload is available");
        Assert.That(_actualShip!.Code, Is.EqualTo(expectedCode), "Ship code mismatch");
    }

    [Then(@"the payload should contain the new ship id (\d+)")]
    public void ThenThePayloadShouldContainTheNewShipId(int expectedId)
    {
        Assert.That(_actualCreatedShipId, Is.Not.Null, "Created ship identifier was not captured");
        Assert.That(_actualCreatedShipId, Is.EqualTo(expectedId), "Created ship identifier mismatch");
    }
}
