using Microsoft.Data.SqlClient;
using System.Data;

namespace PortalFinanceiro.Infrastructure.Data.Providers;

public class SqlServerConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
