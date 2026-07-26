using System.Data;

namespace PortalFinanceiro.Infrastructure.Data;

public interface IDatabaseConnectionFactory
{
    IDbConnection CreateConnection();
}
