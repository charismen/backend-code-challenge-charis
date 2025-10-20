using System.Data;
using Dapper;

namespace ShipManagement.API.Data
{
    public class DapperExecutor : IDapperExecutor
    {
        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? param = null) =>
            connection.QueryAsync<T>(sql, param);

        public Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string sql, object? param = null) =>
            connection.QueryFirstOrDefaultAsync<T>(sql, param);

        public Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object? param = null) =>
            connection.QuerySingleAsync<T>(sql, param);

        public Task<int> ExecuteAsync(IDbConnection connection, string sql, object? param = null) =>
            connection.ExecuteAsync(sql, param);
    }
}
