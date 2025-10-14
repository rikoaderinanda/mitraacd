using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace mitraacd.Services
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
    public class NpgsqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public NpgsqlConnectionFactory(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
            => new NpgsqlConnection(_connectionString);
    }
}