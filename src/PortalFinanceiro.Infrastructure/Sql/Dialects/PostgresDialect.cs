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
    public string FirstRow(string selectList, string fromClause, string whereClause, string orderBy)
        => $"SELECT {selectList} FROM {fromClause} WHERE {whereClause} ORDER BY {orderBy} LIMIT 1";
}
