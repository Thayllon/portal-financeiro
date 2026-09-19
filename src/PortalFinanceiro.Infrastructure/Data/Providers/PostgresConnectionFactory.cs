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

    public IDbConnection CreateConnection()
    {
        var cs = _connectionString;
        // Neon inclui channel_binding=require que o Npgsql 9 ainda não reconhece -> remove
        if (cs.Contains("channel_binding", StringComparison.OrdinalIgnoreCase))
        {
            cs = System.Text.RegularExpressions.Regex.Replace(cs, @"[&?]?channel_binding=[^&]*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cs = cs.Replace("?&", "?").Replace("&&", "&").TrimEnd('?', '&');
            // se ficou "?&" ou "?": limpar
            if (cs.EndsWith("?")) cs = cs[..^1];
        }
        return new NpgsqlConnection(cs);
    }
}
