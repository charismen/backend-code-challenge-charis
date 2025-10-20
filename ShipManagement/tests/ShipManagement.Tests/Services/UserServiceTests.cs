using System.Data;
using System.Linq;
using NSubstitute;
using ShipManagement.API.Data;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;
using ShipManagement.API.Services;
using Xunit;

namespace ShipManagement.Tests.Services;

public class UserServiceTests
{
    private static (UserService service, IDapperExecutor dapper, IDbConnection connection) CreateService()
    {
        var factory = Substitute.For<IDatabaseConnectionFactory>();
        var connection = Substitute.For<IDbConnection>();
        factory.CreateConnection().Returns(connection);
        var dapper = Substitute.For<IDapperExecutor>();
        return (new UserService(factory, dapper), dapper, connection);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsUsers()
    {
        var (service, dapper, connection) = CreateService();
        var users = new[] { new User { UserId = 1, Name = "Alice" } }.AsEnumerable();
        dapper.QueryAsync<User>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult<IEnumerable<User>>(users));

        var result = await service.GetAllUsersAsync();

        Assert.Equal(users, result);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser()
    {
        var (service, dapper, connection) = CreateService();
        var user = new User { UserId = 1, Name = "Alice" };
        dapper.QueryFirstOrDefaultAsync<User>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult<User?>(user));

        var result = await service.GetUserByIdAsync(1);

        Assert.Equal(user, result);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsCreatedUser()
    {
        var (service, dapper, connection) = CreateService();
        var user = new User { UserId = 5, Name = "Bob" };
        dapper.QuerySingleAsync<User>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult(user));

        var created = await service.CreateUserAsync(new User { Name = "Bob" });

        Assert.Equal(user, created);
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsNotFound_WhenSqlException()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryFirstOrDefaultAsync<User>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<User?>(SqlExceptionFactory.Create("User not found")));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateUserAsync(new User { UserId = 1, Name = "Alice" }));
    }

    [Fact]
    public async Task DeleteUserAsync_ThrowsNotFound_WhenSqlException()
    {
        var (service, dapper, connection) = CreateService();
        dapper.ExecuteAsync(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<int>(SqlExceptionFactory.Create("User not found")));

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteUserAsync(1));
    }

    [Fact]
    public async Task AssignShipToUserAsync_ReturnsFalse_WhenNoAssignment()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryFirstOrDefaultAsync<dynamic>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult<dynamic?>(null));

        var result = await service.AssignShipToUserAsync(1, "SHIP01");

        Assert.False(result);
    }

    [Fact]
    public async Task AssignShipToUserAsync_ThrowsConflict_WhenDuplicate()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryFirstOrDefaultAsync<dynamic>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<dynamic?>(SqlExceptionFactory.Create("Assignment already exists")));

        await Assert.ThrowsAsync<ConflictException>(() => service.AssignShipToUserAsync(1, "SHIP01"));
    }

    [Fact]
    public async Task AssignShipToUserAsync_ThrowsNotFound_WhenUserMissing()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryFirstOrDefaultAsync<dynamic>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<dynamic?>(SqlExceptionFactory.Create("User not found")));

        await Assert.ThrowsAsync<NotFoundException>(() => service.AssignShipToUserAsync(1, "SHIP01"));
    }

    [Fact]
    public async Task AssignShipToUserAsync_ThrowsNotFound_WhenShipMissing()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryFirstOrDefaultAsync<dynamic>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<dynamic?>(SqlExceptionFactory.Create("Ship not found")));

        await Assert.ThrowsAsync<NotFoundException>(() => service.AssignShipToUserAsync(1, "SHIP01"));
    }

    [Fact]
    public async Task RemoveShipFromUserAsync_ThrowsNotFound_WhenMissing()
    {
        var (service, dapper, connection) = CreateService();
        dapper.ExecuteAsync(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<int>(SqlExceptionFactory.Create("Assignment not found")));

        await Assert.ThrowsAsync<NotFoundException>(() => service.RemoveShipFromUserAsync(1, "SHIP01"));
    }

    [Fact]
    public async Task GetShipsByUserAsync_ThrowsNotFound_WhenNoAssignments()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryAsync<Ship>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromResult<IEnumerable<Ship>>(Array.Empty<Ship>()));

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetShipsByUserAsync(1));
    }

    [Fact]
    public async Task GetShipsByUserAsync_ThrowsNotFound_WhenUserMissing()
    {
        var (service, dapper, connection) = CreateService();
        dapper.QueryAsync<Ship>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(Task.FromException<IEnumerable<Ship>>(SqlExceptionFactory.Create("User not found")));

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetShipsByUserAsync(1));
    }
}
