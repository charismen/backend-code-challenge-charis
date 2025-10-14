using System.Data;

namespace ShipManagement.API.Data
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}