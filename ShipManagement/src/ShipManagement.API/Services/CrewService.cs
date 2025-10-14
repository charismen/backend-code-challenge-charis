using Dapper;
using ShipManagement.API.Data;
using ShipManagement.API.Models;
using System.Data;

namespace ShipManagement.API.Services
{
    public class CrewService : ICrewService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public CrewService(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PagedResult<CrewMember>> GetCrewListAsync(CrewListRequest request)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var parameters = new DynamicParameters();
            parameters.Add("@ShipId", request.ShipId);
            parameters.Add("@PageNumber", request.PageNumber);
            parameters.Add("@PageSize", request.PageSize);
            parameters.Add("@SortBy", request.SortBy);
            parameters.Add("@SortDescending", request.SortDescending);
            parameters.Add("@StatusFilter", request.StatusFilter);
            parameters.Add("@NameFilter", request.NameFilter);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var crewMembers = await connection.QueryAsync<CrewMember>(
                "EXEC GetCrewList @ShipId, @PageNumber, @PageSize, @SortBy, @SortDescending, @StatusFilter, @NameFilter, @TotalCount OUTPUT",
                parameters);

            var totalCount = parameters.Get<int>("@TotalCount");

            return new PagedResult<CrewMember>
            {
                Items = crewMembers,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}