using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class CategoriaHistoricoRepository : SqlBaseRepository, ICategoriaHistoricoRepository
{
    public CategoriaHistoricoRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task InserirAsync(CategoriaHistorico entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaHistoricoSql.Inserir, entity));

    public async Task<IEnumerable<CategoriaHistorico>> ListarPorCategoriaAsync(Guid idCategoria, ETipoCategoria tipoCategoria)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<CategoriaHistorico>(conn, CategoriaHistoricoSql.ListarPorCategoria, new { IdCategoria = idCategoria, TipoCategoria = (int)tipoCategoria }));
}
