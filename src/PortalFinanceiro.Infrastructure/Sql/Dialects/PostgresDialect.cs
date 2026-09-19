namespace PortalFinanceiro.Infrastructure.Sql.Dialects;

public class PostgresDialect : ISqlDialect
{
    public string SchemaPrefix => "";
    public string BooleanTrue => "TRUE";
    public string BooleanFalse => "FALSE";
    public string CurrentTimestamp => "CURRENT_TIMESTAMP";
    public string UtcTimestamp => "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'";
    public string YearOf(string column) => $"EXTRACT(YEAR FROM {column})";
    public string MonthOf(string column) => $"EXTRACT(MONTH FROM {column})";
    public string Like(string column, string paramName) => $"{column} ILIKE '%' || {paramName} || '%'";
}
