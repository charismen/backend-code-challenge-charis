using System.Data;
using System.Linq;
using NSubstitute;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;
using ShipManagement.API.Services;
using Xunit;

namespace ShipManagement.Tests.Services;

public class FinancialServiceTests
{
    private static (FinancialService service, IDapperExecutor dapper, IDbConnection connection) CreateService()
    {
        var factory = Substitute.For<IDatabaseConnectionFactory>();
        var connection = Substitute.For<IDbConnection>();
        factory.CreateConnection().Returns(connection);
        var dapper = Substitute.For<IDapperExecutor>();
        return (new FinancialService(factory, dapper), dapper, connection);
    }

    [Fact]
    public async Task GetFinancialReportDetailAsync_ReturnsItems()
    {
        var (service, dapper, connection) = CreateService();
        var items = new[] { new FinancialReportItem { AccountNumber = "A001" } }.AsEnumerable();
        dapper.QueryAsync<FinancialReportItem>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult<IEnumerable<FinancialReportItem>>(items));

        var result = await service.GetFinancialReportDetailAsync(new FinancialReportRequest { ShipCode = "SHIP01", Year = 2024, Month = 3 });

        Assert.Equal(items, result);
    }

    [Fact]
    public async Task GetFinancialReportDetailAsync_ThrowsNotFound_WhenShipMissing()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryAsync<FinancialReportItem>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<IEnumerable<FinancialReportItem>>(SqlExceptionFactory.Create("Ship not found")));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetFinancialReportDetailAsync(new FinancialReportRequest { ShipCode = "SHIP01", Year = 2024, Month = 3 }));
    }

    [Fact]
    public async Task GetFinancialReportSummaryAsync_ReturnsItems()
    {
        var (service, dapper, connection) = CreateService();
        var items = new[] { new FinancialReportItem { AccountNumber = "A100" } }.AsEnumerable();
        dapper.QueryAsync<FinancialReportItem>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult<IEnumerable<FinancialReportItem>>(items));

        var result = await service.GetFinancialReportSummaryAsync(new FinancialReportRequest { ShipCode = "SHIP01", Year = 2024, Month = 3 });

        Assert.Equal(items, result);
    }

    [Fact]
    public async Task GetFinancialReportSummaryAsync_ThrowsNotFound_WhenShipMissing()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryAsync<FinancialReportItem>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<IEnumerable<FinancialReportItem>>(SqlExceptionFactory.Create("Ship not found")));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetFinancialReportSummaryAsync(new FinancialReportRequest { ShipCode = "SHIP01", Year = 2024, Month = 3 }));
    }
}
