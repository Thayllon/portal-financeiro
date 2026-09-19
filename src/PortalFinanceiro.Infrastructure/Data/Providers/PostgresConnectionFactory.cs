using System.Data;
using Npgsql;

namespace PortalFinanceiro.Infrastructure.Data.Providers;

public class PostgresConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
