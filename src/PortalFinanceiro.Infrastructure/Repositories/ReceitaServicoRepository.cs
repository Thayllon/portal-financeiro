using Dapper;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ReceitaServicoRepository : SqlBaseRepository, IReceitaServicoRepository
{
    public ReceitaServicoRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<ReceitaServico>> ListarPorReceitaAsync(Guid receitaId)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ReceitaServico>(conn, ReceitaServicoSql.ListarPorReceitaId, new { ReceitaId = receitaId }));

    public async Task InserirAsync(ReceitaServico entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaServicoSql.Inserir, entity));

    public async Task InserirEmMassaAsync(IEnumerable<ReceitaServico> entities)
    {
        await ExecuteWithConnectionAsync(async conn =>
        {
            using var tx = conn.BeginTransaction();
            foreach (var entity in entities)
                await conn.ExecuteAsync(ReceitaServicoSql.Inserir, entity, tx);
            tx.Commit();
            return 0;
        });
    }

    public async Task ExcluirPorReceitaAsync(Guid receitaId)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaServicoSql.ExcluirPorReceitaId, new { ReceitaId = receitaId }));
}
