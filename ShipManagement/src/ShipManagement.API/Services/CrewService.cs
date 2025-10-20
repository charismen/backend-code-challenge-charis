using Dapper;
using ShipManagement.API.Data;
using ShipManagement.API.Models;
using System.Data;

namespace ShipManagement.API.Services
{
    public class CrewService : ICrewService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IDapperExecutor _dapper;

        public CrewService(IDatabaseConnectionFactory connectionFactory, IDapperExecutor dapper)
        {
            _connectionFactory = connectionFactory;
            _dapper = dapper;
        }

        public async Task<PagedResult<CrewMember>> GetCrewListAsync(CrewListRequest request)
        {
            using var connection = _connectionFactory.CreateConnection();

            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var parameters = new DynamicParameters();
            parameters.Add("@ShipCode", request.ShipCode);
            parameters.Add("@SearchTerm", request.SearchTerm);
            parameters.Add("@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? "RankName" : request.SortColumn);
            parameters.Add("@SortDirection", request.SortDescending ? "DESC" : "ASC");
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@StatusFilter", request.StatusFilter);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var crewMembers = await _dapper.QueryAsync<CrewMember>(
                connection,
                "EXEC GetCrewList @ShipCode, @SearchTerm, @SortColumn, @SortDirection, @PageNumber, @PageSize, @StatusFilter, @TotalCount OUTPUT",
                parameters);

            var totalCount = parameters.Get<int>("@TotalCount");

            return new PagedResult<CrewMember>
            {
                Items = crewMembers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
