using Npgsql;
using Ordering.Application.Data;
using System.Data;

namespace Ordering.Infrastructure.Data
{
    public class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
