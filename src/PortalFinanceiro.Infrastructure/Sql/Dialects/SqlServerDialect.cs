namespace PortalFinanceiro.Infrastructure.Sql.Dialects;

public class SqlServerDialect : ISqlDialect
{
    public string SchemaPrefix => "dbo.";
    public string BooleanTrue => "1";
    public string BooleanFalse => "0";
    public string CurrentTimestamp => "CURRENT_TIMESTAMP";
    public string UtcTimestamp => "GETUTCDATE()";
    public string YearOf(string column) => $"YEAR({column})";
    public string MonthOf(string column) => $"MONTH({column})";
    public string Like(string column, string paramName) => $"{column} LIKE '%' + {paramName} + '%'";
    public string FirstRow(string selectList, string fromClause, string whereClause, string orderBy)
        => $"SELECT TOP 1 {selectList} FROM {fromClause} WHERE {whereClause} ORDER BY {orderBy}";
}
