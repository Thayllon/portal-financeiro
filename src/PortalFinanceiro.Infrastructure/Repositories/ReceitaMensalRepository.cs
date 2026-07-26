using Dapper;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ReceitaMensalRepository : SqlBaseRepository, IReceitaMensalRepository
{
    public ReceitaMensalRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<ReceitaMensal?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ReceitaMensal>(conn, ReceitaMensalSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<ReceitaMensal>> ListarPorMesAsync(Guid idUsuario, int mes, int ano)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ReceitaMensal>(conn, ReceitaMensalSql.ListarPorMes, new { IdUsuario = idUsuario, Mes = mes, Ano = ano }));

    public async Task<IEnumerable<ReceitaMensal>> ListarPorReceitaRecorrenteAsync(Guid idReceitaRecorrente)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ReceitaMensal>(conn, ReceitaMensalSql.ListarPorReceitaRecorrente, new { IdReceitaRecorrente = idReceitaRecorrente }));

    public async Task InserirAsync(ReceitaMensal entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaMensalSql.Inserir, entity));

    public async Task InserirEmMassaAsync(IEnumerable<ReceitaMensal> entities)
    {
        await ExecuteWithConnectionAsync(async conn =>
        {
            conn.Open();
            using var tx = conn.BeginTransaction();
            foreach (var entity in entities)
                await conn.ExecuteAsync(ReceitaMensalSql.Inserir, entity, tx);
            tx.Commit();
            return 0;
        });
    }

    public async Task AtualizarAsync(ReceitaMensal entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaMensalSql.Atualizar, entity));
}
