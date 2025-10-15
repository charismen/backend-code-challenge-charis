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
            parameters.Add("@ShipCode", request.ShipCode);
            parameters.Add("@SearchTerm", request.SearchTerm);
            parameters.Add("@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? "RankName" : request.SortColumn);
            parameters.Add("@SortDirection", request.SortDescending ? "DESC" : "ASC");
            parameters.Add("@PageNumber", request.PageNumber);
            parameters.Add("@PageSize", request.PageSize);
            parameters.Add("@StatusFilter", request.StatusFilter);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var crewMembers = await connection.QueryAsync<CrewMember>(
                "EXEC GetCrewList @ShipCode, @SearchTerm, @SortColumn, @SortDirection, @PageNumber, @PageSize, @StatusFilter, @TotalCount OUTPUT",
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
