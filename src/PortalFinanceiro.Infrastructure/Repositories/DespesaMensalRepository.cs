using Dapper;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class DespesaMensalRepository : SqlBaseRepository, IDespesaMensalRepository
{
    public DespesaMensalRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<DespesaMensal?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<DespesaMensal>(conn, DespesaMensalSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<DespesaMensal>> ListarPorMesAsync(Guid idUsuario, int mes, int ano)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<DespesaMensal>(conn, DespesaMensalSql.ListarPorMes, new { IdUsuario = idUsuario, Mes = mes, Ano = ano }));

    public async Task<IEnumerable<DespesaMensal>> ListarPorDespesaRecorrenteAsync(Guid idDespesaRecorrente)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<DespesaMensal>(conn, DespesaMensalSql.ListarPorDespesaRecorrente, new { IdDespesaRecorrente = idDespesaRecorrente }));

    public async Task InserirAsync(DespesaMensal entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, DespesaMensalSql.Inserir, entity));

    public async Task InserirEmMassaAsync(IEnumerable<DespesaMensal> entities)
    {
        await ExecuteWithConnectionAsync(async conn =>
        {
            conn.Open();
            using var tx = conn.BeginTransaction();
            foreach (var entity in entities)
                await conn.ExecuteAsync(DespesaMensalSql.Inserir, entity, tx);
            tx.Commit();
            return 0;
        });
    }

    public async Task AtualizarAsync(DespesaMensal entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, DespesaMensalSql.Atualizar, entity));
}
