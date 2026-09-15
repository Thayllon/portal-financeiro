using Dapper;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class DespesaServicoRepository : SqlBaseRepository, IDespesaServicoRepository
{
    public DespesaServicoRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<DespesaServico>> ListarPorDespesaAsync(Guid despesaId)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<DespesaServico>(conn, DespesaServicoSql.ListarPorDespesaId, new { DespesaId = despesaId }));

    public async Task InserirAsync(DespesaServico entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, DespesaServicoSql.Inserir, entity));

    public async Task InserirEmMassaAsync(IEnumerable<DespesaServico> entities)
    {
        await ExecuteWithConnectionAsync(async conn =>
        {
            using var tx = conn.BeginTransaction();
            foreach (var entity in entities)
                await conn.ExecuteAsync(DespesaServicoSql.Inserir, entity, tx);
            tx.Commit();
            return 0;
        });
    }

    public async Task ExcluirPorDespesaAsync(Guid despesaId)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, DespesaServicoSql.ExcluirPorDespesaId, new { DespesaId = despesaId }));
}
