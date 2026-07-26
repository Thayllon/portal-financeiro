namespace PortalFinanceiro.Infrastructure.Sql;

public interface ISqlDialect
{
    string SchemaPrefix { get; }
}

public static class SqlDialect
{
    public static ISqlDialect Current { get; private set; } = new Dialects.SqlServerDialect();

    public static void Configure(ISqlDialect dialect)
    {
        Current = dialect;
    }
}
