using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace ShipManagement.Tests.Financial;

[Binding]
[Scope(Feature = "Financial reporting")]
public class FinancialStepDefinitions
{
    private readonly IFinancialService _financialService;
    private readonly ILogger<FinancialController> _logger;
    private readonly FinancialController _controller;

    private FinancialReportRequest _financialReportRequest = null!;
    private ActionResult<IEnumerable<FinancialReportItem>>? _financialReportResponse;
    private IActionResult? _lastActionResult;
    private IEnumerable<FinancialReportItem>? _actualFinancialItems;

    public FinancialStepDefinitions()
    {
        _financialService = Substitute.For<IFinancialService>();
        _logger = Substitute.For<ILogger<FinancialController>>();
        _controller = new FinancialController(_financialService, _logger);
    }

    [Given(@"the financial report request targets ship id (\d+) for (January|February|March|April|May|June|July|August|September|October|November|December) (\d{4})")]
    public void GivenTheFinancialReportRequestTargetsShipIdForMonthName(int shipId, string monthName, int year)
    {
        var month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture).Month;

        _financialReportRequest = new FinancialReportRequest
        {
            ShipId = shipId,
            Year = year,
            Month = month
        };

        var items = new List<FinancialReportItem>
        {
            new FinancialReportItem
            {
                AccountId = 1,
                AccountCode = "A001",
                AccountName = "Fuel",
                ActualAmount = 5000,
                BudgetAmount = 4500,
                Variance = 500,
                YTDActual = 30000,
                YTDBudget = 27000,
                YTDVariance = 3000
            }
        };

        _financialService.GetFinancialReportDetailAsync(Arg.Any<FinancialReportRequest>())
            .Returns(Task.FromResult<IEnumerable<FinancialReportItem>>(items));
        _financialService.GetFinancialReportSummaryAsync(Arg.Any<FinancialReportRequest>())
            .Returns(Task.FromResult<IEnumerable<FinancialReportItem>>(items));
    }

    [Given(@"the financial report request targets ship id (\d+) for month (\d{1,2}) of (\d{4})")]
    public void GivenTheFinancialReportRequestTargetsShipIdForMonthNumber(int shipId, int month, int year)
    {
        _financialReportRequest = new FinancialReportRequest
        {
            ShipId = shipId,
            Year = year,
            Month = month
        };
    }

    [When(@"the financial report detail is requested")]
    public async Task WhenTheFinancialReportDetailIsRequested()
    {
        _financialReportResponse = await _controller.GetFinancialReportDetail(_financialReportRequest);
        _lastActionResult = _financialReportResponse.Result;
    }

    [When(@"the financial report summary is requested")]
    public async Task WhenTheFinancialReportSummaryIsRequested()
    {
        _financialReportResponse = await _controller.GetFinancialReportSummary(_financialReportRequest);
        _lastActionResult = _financialReportResponse.Result;
    }

    [Given(@"the financial detail service fails")]
    public void GivenTheFinancialDetailServiceFails()
    {
        _financialService.GetFinancialReportDetailAsync(Arg.Any<FinancialReportRequest>())
            .ThrowsAsync(new InvalidOperationException("Financial detail failure"));
    }

    [Given(@"the financial summary service fails")]
    public void GivenTheFinancialSummaryServiceFails()
    {
        _financialService.GetFinancialReportSummaryAsync(Arg.Any<FinancialReportRequest>())
            .ThrowsAsync(new InvalidOperationException("Financial summary failure"));
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
                _actualFinancialItems = okResult!.Value as IEnumerable<FinancialReportItem>;
                Assert.That(_actualFinancialItems, Is.Not.Null, "Expected financial report items in OK response");
                break;

            case "BadRequest":
                Assert.That(_lastActionResult, Is.TypeOf<BadRequestObjectResult>(),
                    "Expected BadRequest result but received a different response");
                _actualFinancialItems = null;
                break;
            case "InternalServerError":
                var objectResult = _lastActionResult as ObjectResult;
                Assert.That(objectResult, Is.Not.Null, "Expected ObjectResult for InternalServerError");
                Assert.That(objectResult!.StatusCode, Is.EqualTo(500), "Expected status code 500");
                _actualFinancialItems = null;
                break;

            default:
                Assert.Fail($"Unsupported status expectation: '{expectedStatus}'");
                break;
        }
    }

    [Then(@"the financial report should contain (\d+) item")]
    public void ThenTheFinancialReportShouldContainItem(int expectedCount)
    {
        Assert.That(_actualFinancialItems, Is.Not.Null, "Financial report payload is not available");
        Assert.That(_actualFinancialItems!.Count(), Is.EqualTo(expectedCount), "Financial report item count mismatch");
    }
}
