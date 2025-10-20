using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using Dapper;
using NSubstitute;
using ShipManagement.API.Data;
using ShipManagement.API.Models;
using ShipManagement.API.Services;
using Xunit;

namespace ShipManagement.Tests.Services;

public class CrewServiceTests
{
    [Fact]
    public async Task GetCrewListAsync_NormalisesPagingAndReturnsResult()
    {
        var factory = Substitute.For<IDatabaseConnectionFactory>();
        var connection = Substitute.For<IDbConnection>();
        factory.CreateConnection().Returns(connection);

        var dapper = Substitute.For<IDapperExecutor>();
        var expected = new[]
        {
            new CrewMember { CrewMemberId = "CREW001", RankName = "Master" }
        }.AsEnumerable();

        dapper.QueryAsync<CrewMember>(connection, Arg.Any<string>(), Arg.Any<object?>())
            .Returns(callInfo =>
            {
                if (callInfo.ArgAt<object?>(2) is DynamicParameters parameters)
                {
                    parameters.Add("@TotalCount", 5, dbType: DbType.Int32, direction: ParameterDirection.Output);
                }

                return Task.FromResult<IEnumerable<CrewMember>>(expected);
            });

        var service = new CrewService(factory, dapper);

        var request = new CrewListRequest { ShipCode = "SHIP01", PageNumber = 0, PageSize = 0 };

        var result = await service.GetCrewListAsync(request);

        Assert.Equal(expected, result.Items);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }
}
