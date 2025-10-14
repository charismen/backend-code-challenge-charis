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

namespace ShipManagement.Tests.Crew;

[Binding]
[Scope(Feature = "Crew management")]
public class CrewStepDefinitions
{
    private readonly ICrewService _crewService;
    private readonly ILogger<CrewController> _logger;
    private readonly CrewController _controller;
    private CrewListRequest _crewListRequest = null!;
    private ActionResult<PagedResult<CrewMember>>? _crewListResponse;
    private PagedResult<CrewMember>? _expectedCrewList;
    private PagedResult<CrewMember>? _actualCrewList;

    public CrewStepDefinitions()
    {
        _crewService = Substitute.For<ICrewService>();
        _logger = Substitute.For<ILogger<CrewController>>();
        _controller = new CrewController(_crewService, _logger);
    }

    [Given(@"the crew list request has ship id (.*), page number (.*), and page size (.*)")]
    public void GivenTheCrewListRequestHasShipIdPageNumberAndPageSize(int shipId, int pageNumber, int pageSize)
    {
        _crewListRequest = new CrewListRequest
        {
            ShipId = shipId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        if (shipId <= 0)
        {
            _expectedCrewList = null;
            return;
        }

        var crewMembers = new List<CrewMember>
        {
            new CrewMember { Id = 1, ShipId = shipId, FirstName = "John", LastName = "Doe", Status = "Onboard" },
            new CrewMember { Id = 2, ShipId = shipId, FirstName = "Jane", LastName = "Smith", Status = "Onboard" }
        };

        _expectedCrewList = new PagedResult<CrewMember>
        {
            Items = crewMembers,
            TotalCount = crewMembers.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _crewService.GetCrewListAsync(Arg.Any<CrewListRequest>())
            .Returns(Task.FromResult(_expectedCrewList)!);
    }

    [Given(@"the crew service fails while retrieving the crew list")]
    public void GivenTheCrewServiceFailsWhileRetrievingTheCrewList()
    {
        _crewService.GetCrewListAsync(Arg.Any<CrewListRequest>())
            .ThrowsAsync(new InvalidOperationException("Crew service failure"));
    }

    [When(@"the crew list is requested")]
    public async Task WhenTheCrewListIsRequested()
    {
        _crewListResponse = await _controller.GetCrewList(_crewListRequest);
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(string expectedStatus)
    {
        Assert.That(_crewListResponse, Is.Not.Null, "Crew list response was not captured");

        var actionResult = _crewListResponse!.Result;
        switch (expectedStatus)
        {
            case "OK":
                var okResult = actionResult as OkObjectResult;
                Assert.That(okResult, Is.Not.Null, "Expected OK result but received a different response");
                _actualCrewList = okResult!.Value as PagedResult<CrewMember>;
                Assert.That(_actualCrewList, Is.Not.Null, "Expected a crew list payload in the OK response");
                break;
            case "BadRequest":
                Assert.That(actionResult, Is.TypeOf<BadRequestObjectResult>(),
                    "Expected BadRequest result but received a different response");
                _actualCrewList = null;
                break;
            case "InternalServerError":
                var objectResult = actionResult as ObjectResult;
                Assert.That(objectResult, Is.Not.Null, "Expected ObjectResult for InternalServerError");
                Assert.That(objectResult!.StatusCode, Is.EqualTo(500), "Expected status code 500");
                _actualCrewList = null;
                break;
            default:
                Assert.Fail($"Unsupported status expectation: '{expectedStatus}'");
                break;
        }
    }

    [Then(@"the crew result should contain (.*) members")]
    public void ThenTheCrewResultShouldContainMembers(int expectedCount)
    {
        Assert.That(_actualCrewList, Is.Not.Null, "Crew list payload is not available");
        var actualCount = _actualCrewList!.Items.Count();
        Assert.That(actualCount, Is.EqualTo(expectedCount), "Crew member count mismatch");
    }

    [Then(@"the total crew count should be (.*)")]
    public void ThenTheTotalCrewCountShouldBe(int expectedTotal)
    {
        Assert.That(_actualCrewList, Is.Not.Null, "Crew list payload is not available");
        Assert.That(_actualCrewList!.TotalCount, Is.EqualTo(expectedTotal), "Total crew count mismatch");
    }
}
