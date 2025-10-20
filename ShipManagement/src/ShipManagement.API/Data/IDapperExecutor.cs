using System.Data;

namespace ShipManagement.API.Data
{
    public interface IDapperExecutor
    {
        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? param = null);
        Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string sql, object? param = null);
        Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object? param = null);
        Task<int> ExecuteAsync(IDbConnection connection, string sql, object? param = null);
    }
}
