namespace PortalFinanceiro.Infrastructure.Sql.Dialects;

public class SqlServerDialect : ISqlDialect
{
    public string SchemaPrefix => "dbo.";
}
