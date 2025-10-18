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

public class FinancialControllerTests
{
    [Fact]
    public async Task GetFinancialReportDetail_ReturnsOk_WhenRequestIsValid()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "SHIP01", Year = 2023, Month = 6 };
        var items = new List<FinancialReportItem>
        {
            new()
            {
                AccountDescription = "Fuel",
                AccountNumber = "A001",
                ActualValue = 5000,
                BudgetValue = 4500,
                VarianceActual = 500
            }
        }.AsEnumerable();

        service.GetFinancialReportDetailAsync(request).Returns(Task.FromResult(items));

        var response = await controller.GetFinancialReportDetail(request);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsAssignableFrom<IEnumerable<FinancialReportItem>>(okResult.Value);
        Assert.Single(payload);
        Assert.Equal("A001", payload.First().AccountNumber);
    }

    [Fact]
    public async Task GetFinancialReportDetail_ReturnsBadRequest_WhenRequestInvalid()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = string.Empty, Year = 2023, Month = 13 };

        var response = await controller.GetFinancialReportDetail(request);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        await service.DidNotReceiveWithAnyArgs().GetFinancialReportDetailAsync(default!);
    }

    [Fact]
    public async Task GetFinancialReportDetail_ReturnsInternalServerError_WhenServiceThrows()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "SHIP01", Year = 2023, Month = 6 };

        service.GetFinancialReportDetailAsync(Arg.Any<FinancialReportRequest>())
            .ThrowsAsync(new InvalidOperationException("detail failure"));

        var response = await controller.GetFinancialReportDetail(request);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetFinancialReportDetail_ReturnsUnauthorized_WhenServiceThrowsUnauthorized()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "SHIP01", Year = 2023, Month = 6 };

        service.GetFinancialReportDetailAsync(Arg.Any<FinancialReportRequest>())
            .ThrowsAsync(new UnauthorizedAccessException());

        var response = await controller.GetFinancialReportDetail(request);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        Assert.Equal("Unauthorized access", unauthorized.Value);
    }

    [Fact]
    public async Task GetFinancialReportDetail_ReturnsNotFound_WhenShipMissing()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "MISSING", Year = 2023, Month = 6 };

        service.GetFinancialReportDetailAsync(Arg.Any<FinancialReportRequest>())
            .ThrowsAsync(new NotFoundException("Ship with code MISSING not found"));

        var response = await controller.GetFinancialReportDetail(request);

        var notFound = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("Ship with code MISSING not found", notFound.Value);
    }

    [Fact]
    public async Task GetFinancialReportSummary_ReturnsOk_WhenRequestIsValid()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "SHIP01", Year = 2023, Month = 6 };
        var items = new[]
        {
            new FinancialReportItem
            {
                AccountDescription = "Maintenance",
                AccountNumber = "A002",
                ActualValue = 2000,
                BudgetValue = 1800,
                VarianceActual = 200
            }
        }.AsEnumerable();

        service.GetFinancialReportSummaryAsync(request).Returns(Task.FromResult(items));

        var response = await controller.GetFinancialReportSummary(request);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsAssignableFrom<IEnumerable<FinancialReportItem>>(okResult.Value);
        Assert.Single(payload);
        Assert.Equal("A002", payload.First().AccountNumber);
    }

    [Fact]
    public async Task GetFinancialReportSummary_ReturnsBadRequest_WhenRequestInvalid()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "SHIP01", Year = 2023, Month = 0 };

        var response = await controller.GetFinancialReportSummary(request);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        await service.DidNotReceiveWithAnyArgs().GetFinancialReportSummaryAsync(default!);
    }

    [Fact]
    public async Task GetFinancialReportSummary_ReturnsInternalServerError_WhenServiceThrows()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "SHIP01", Year = 2023, Month = 6 };

        service.GetFinancialReportSummaryAsync(Arg.Any<FinancialReportRequest>())
            .ThrowsAsync(new InvalidOperationException("summary failure"));

        var response = await controller.GetFinancialReportSummary(request);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetFinancialReportSummary_ReturnsUnauthorized_WhenServiceThrowsUnauthorized()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "SHIP01", Year = 2023, Month = 6 };

        service.GetFinancialReportSummaryAsync(Arg.Any<FinancialReportRequest>())
            .ThrowsAsync(new UnauthorizedAccessException());

        var response = await controller.GetFinancialReportSummary(request);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        Assert.Equal("Unauthorized access", unauthorized.Value);
    }

    [Fact]
    public async Task GetFinancialReportSummary_ReturnsNotFound_WhenShipMissing()
    {
        var service = Substitute.For<IFinancialService>();
        var logger = Substitute.For<ILogger<FinancialController>>();
        var controller = new FinancialController(service, logger);
        var request = new FinancialReportRequest { ShipCode = "MISSING", Year = 2023, Month = 6 };

        service.GetFinancialReportSummaryAsync(Arg.Any<FinancialReportRequest>())
            .ThrowsAsync(new NotFoundException("Ship with code MISSING not found"));

        var response = await controller.GetFinancialReportSummary(request);

        var notFound = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("Ship with code MISSING not found", notFound.Value);
    }
}
