using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ShipManagement.API.Controllers;
using ShipManagement.API.Models;
using ShipManagement.API.Services;
using Xunit;

namespace ShipManagement.Tests.Controllers;

public class CrewControllerTests
{
    [Fact]
    public async Task GetCrewList_ReturnsOk_WhenRequestIsValid()
    {
        var service = Substitute.For<ICrewService>();
        var logger = Substitute.For<ILogger<CrewController>>();
        var controller = new CrewController(service, logger);
        var request = new CrewListRequest { ShipId = 1, PageNumber = 1, PageSize = 10 };
        var expected = new PagedResult<CrewMember>
        {
            Items = new[]
            {
                new CrewMember { Id = 1, ShipId = 1, FirstName = "John", LastName = "Doe" },
                new CrewMember { Id = 2, ShipId = 1, FirstName = "Jane", LastName = "Smith" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        service.GetCrewListAsync(request).Returns(Task.FromResult(expected));

        var response = await controller.GetCrewList(request);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<PagedResult<CrewMember>>(okResult.Value);
        Assert.Equal(expected.TotalCount, payload.TotalCount);
        Assert.Equal(expected.Items.Count(), payload.Items.Count());
        Assert.Equal(expected.PageNumber, payload.PageNumber);
        Assert.Equal(expected.PageSize, payload.PageSize);
    }

    [Fact]
    public async Task GetCrewList_ReturnsBadRequest_WhenShipIdInvalid()
    {
        var service = Substitute.For<ICrewService>();
        var logger = Substitute.For<ILogger<CrewController>>();
        var controller = new CrewController(service, logger);
        var request = new CrewListRequest { ShipId = 0, PageNumber = 1, PageSize = 10 };

        var response = await controller.GetCrewList(request);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        await service.DidNotReceiveWithAnyArgs().GetCrewListAsync(default!);
    }

    [Fact]
    public async Task GetCrewList_ReturnsInternalServerError_WhenServiceThrows()
    {
        var service = Substitute.For<ICrewService>();
        var logger = Substitute.For<ILogger<CrewController>>();
        var controller = new CrewController(service, logger);
        var request = new CrewListRequest { ShipId = 1, PageNumber = 1, PageSize = 10 };

        service.GetCrewListAsync(Arg.Any<CrewListRequest>())
            .ThrowsAsync(new InvalidOperationException("Crew service failure"));

        var response = await controller.GetCrewList(request);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
