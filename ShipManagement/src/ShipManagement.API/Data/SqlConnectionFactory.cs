using System.Data;
using Microsoft.Data.SqlClient;

namespace ShipManagement.API.Data
{
    public class SqlConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}