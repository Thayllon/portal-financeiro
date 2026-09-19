using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using Polly;
using PortalFinanceiro.Infrastructure.Data;

namespace PortalFinanceiro.Infrastructure.Sql.Base;

public abstract class SqlBaseRepository
{
    private static readonly IAsyncPolicy _sharedRetryPolicy = Policy
        .Handle<SqlException>(ex => ex.Number is -2 or 4060 or 10928 or 10929 or 1205 or 40143 or 11001)
        .Or<NpgsqlException>(ex => ex.SqlState is "08000" or "08001" or "08003" or "08006" or "53300" or "57P01" or "57P03" or "40001" or "40P01")
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));

    protected static readonly int _commandTimeout = 60;
    private readonly IDatabaseConnectionFactory _connectionFactory;

    protected SqlBaseRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected async Task<T> ExecuteWithConnectionAsync<T>(Func<IDbConnection, Task<T>> operation)
    {
        return await _sharedRetryPolicy.ExecuteAsync(async () =>
        {
            using var conn = _connectionFactory.CreateConnection();
            await ((DbConnection)conn).OpenAsync();
            return await operation(conn);
        });
    }

    protected async Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection conn, string sql, object? parameters = null)
        => await conn.QueryFirstOrDefaultAsync<T>(sql, parameters, commandTimeout: _commandTimeout);

    protected async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection conn, string sql, object? parameters = null)
        => await conn.QueryAsync<T>(sql, parameters, commandTimeout: _commandTimeout);

    protected async Task<int> ExecuteAsync(IDbConnection conn, string sql, object? parameters = null)
        => await conn.ExecuteAsync(sql, parameters, commandTimeout: _commandTimeout);
}
