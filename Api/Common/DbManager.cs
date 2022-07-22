using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Api.Common
{
    public interface IDbManager
    {
        IDbConnection CreateConnection();
    }

    public class DbManager : IDbManager
    {
        private readonly string _connectionString;
        public DbManager(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServer");
        }
        
        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}