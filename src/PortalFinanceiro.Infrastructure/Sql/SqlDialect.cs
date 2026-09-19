namespace PortalFinanceiro.Infrastructure.Sql;

public interface ISqlDialect
{
    string SchemaPrefix { get; }
    string BooleanTrue { get; }
    string BooleanFalse { get; }
    string CurrentTimestamp { get; }
    string UtcTimestamp { get; }
    string YearOf(string column);
    string MonthOf(string column);
    string Like(string column, string paramName);
}

public static class SqlDialect
{
    public static ISqlDialect Current { get; private set; } = new Dialects.SqlServerDialect();

    public static void Configure(ISqlDialect dialect)
    {
        Current = dialect;
    }
}
