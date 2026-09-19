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
        var cs = _connectionString?.Trim() ?? "";

        // Se vier no formato URL (postgresql://...), converte para formato Host=... que o Npgsql sempre entende
        // e remove channel_binding que o Npgsql 9 não reconhece
        if (cs.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            cs.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                // remove channel_binding antes de parsear
                if (cs.Contains("channel_binding", StringComparison.OrdinalIgnoreCase))
                {
                    cs = System.Text.RegularExpressions.Regex.Replace(cs, @"[&?]?channel_binding=[^&]*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    cs = cs.Replace("?&", "?").Replace("&&", "&").TrimEnd('?', '&');
                    if (cs.EndsWith("?")) cs = cs[..^1];
                }

                var uri = new Uri(cs);
                var userInfo = uri.UserInfo.Split(':', 2);
                var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port.ToString() : "5432";
                var database = uri.AbsolutePath.Trim('/').Split('?')[0];
                // manter sslmode e outros params válidos (sem channel_binding)
                var query = uri.Query; // já sem channel_binding
                var sslMode = "Require";
                if (query.Contains("sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    var m = System.Text.RegularExpressions.Regex.Match(query, @"sslmode=([^&]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (m.Success) sslMode = m.Groups[1].Value;
                }

                cs = $"Host={host};Port={port};Database={database};Username={username};Password={password};Ssl Mode={sslMode};Trust Server Certificate=true";
            }
            catch
            {
                // fallback: deixa o Npgsql tentar parsear direto
            }
        }
        else if (cs.Contains("channel_binding", StringComparison.OrdinalIgnoreCase))
        {
            cs = System.Text.RegularExpressions.Regex.Replace(cs, @"[&?]?channel_binding=[^&]*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cs = cs.Replace("?&", "?").Replace("&&", "&").TrimEnd('?', '&');
            if (cs.EndsWith("?")) cs = cs[..^1];
        }

        return new NpgsqlConnection(cs);
    }
}
