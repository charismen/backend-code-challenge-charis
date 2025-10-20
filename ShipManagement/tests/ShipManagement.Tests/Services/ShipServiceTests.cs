using System.Data;
using System.Linq;
using NSubstitute;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;
using ShipManagement.API.Services;
using Xunit;

namespace ShipManagement.Tests.Services;

public class ShipServiceTests
{
    private static (ShipService service, IDapperExecutor dapper, IDbConnection connection) CreateService()
    {
        var factory = Substitute.For<IDatabaseConnectionFactory>();
        var connection = Substitute.For<IDbConnection>();
        factory.CreateConnection().Returns(connection);
        var dapper = Substitute.For<IDapperExecutor>();

        return (new ShipService(factory, dapper), dapper, connection);
    }

    [Fact]
    public async Task GetAllShipsAsync_ReturnsShips()
    {
        var (service, dapper, connection) = CreateService();
        var ships = new[] { new Ship { Code = "SHIP1" } }.AsEnumerable();
        dapper.QueryAsync<Ship>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult<IEnumerable<Ship>>(ships));

        var result = await service.GetAllShipsAsync();

        Assert.Equal(ships, result);
        await dapper.Received(1).QueryAsync<Ship>(connection, "EXEC GetShips", null);
    }

    [Fact]
    public async Task GetShipByCodeAsync_ReturnsNull_WhenNotFound()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryFirstOrDefaultAsync<Ship>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult<Ship?>(null));

        var ship = await service.GetShipByCodeAsync("MISSING");

        Assert.Null(ship);
        await dapper.Received(1)
            .QueryFirstOrDefaultAsync<Ship>(connection, "EXEC GetShipByCode @Code", Arg.Any<object?>());
    }

    [Fact]
    public async Task CreateShipAsync_ReturnsShip()
    {
        var (service, dapper, connection) = CreateService();
        var expected = new Ship { Code = "SHIP1" };
        dapper.QuerySingleAsync<Ship>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult(expected));

        var ship = await service.CreateShipAsync(expected);

        Assert.Equal(expected, ship);
    }

    [Fact]
    public async Task CreateShipAsync_ThrowsConflict_WhenDuplicate()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QuerySingleAsync<Ship>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<Ship>(SqlExceptionFactory.Create("Ship with this code already exists")));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateShipAsync(new Ship { Code = "SHIP1" }));
    }

    [Fact]
    public async Task UpdateShipAsync_ThrowsNotFound_WhenShipMissing()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryFirstOrDefaultAsync<Ship>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<Ship?>(SqlExceptionFactory.Create("Ship not found")));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateShipAsync(new Ship { Code = "SHIP1" }));
    }

    [Fact]
    public async Task DeleteShipAsync_ThrowsNotFound_WhenShipMissing()
    {
        var (service, dapper, connection) = CreateService();
        dapper.ExecuteAsync(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<int>(SqlExceptionFactory.Create("Ship not found")));

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteShipAsync("SHIP1"));
    }

    [Fact]
    public async Task DeleteShipAsync_ReturnsTrue_WhenSuccessful()
    {
        var (service, dapper, connection) = CreateService();
        dapper.ExecuteAsync(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult(1));

        var result = await service.DeleteShipAsync("SHIP1");

        Assert.True(result);
    }
}
