using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class VinculosCategoria : SqlBaseRepository, IVinculosCategoria
{
    public VinculosCategoria(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<bool> PossuiVinculosAsync(Guid categoriaId, string tabelaVinculo)
    {
        var sql = $"SELECT COUNT(1) FROM {SqlDialect.Current.SchemaPrefix}{tabelaVinculo} WHERE IdCategoria = @IdCategoria AND Ativo = 1";
        return await ExecuteWithConnectionAsync(async conn =>
        {
            var count = await QueryFirstOrDefaultAsync<int>(conn, sql, new { IdCategoria = categoriaId });
            return count > 0;
        });
    }
}
