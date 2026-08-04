using Dapper;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class DespesaRepository : SqlBaseRepository, IDespesaRepository
{
    public DespesaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Despesa?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Despesa>(conn, DespesaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<Despesa>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<Despesa>(conn, DespesaSql.ListarPorMes, new { IdUsuario = idUsuario, Mes = mes, Ano = ano, IdConta = idConta, Status = status, IdCategoria = idCategoria, Busca = busca }));

    public async Task InserirAsync(Despesa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, DespesaSql.Inserir, entity));

    public async Task InserirEmMassaAsync(IEnumerable<Despesa> entities)
    {
        await ExecuteWithConnectionAsync(async conn =>
        {
            using var tx = conn.BeginTransaction();
            foreach (var entity in entities)
                await conn.ExecuteAsync(DespesaSql.Inserir, entity, tx);
            tx.Commit();
            return 0;
        });
    }

    public async Task AtualizarAsync(Despesa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, DespesaSql.Atualizar, entity));

    public async Task ExcluirAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, $"UPDATE {SqlDialect.Current.SchemaPrefix}Despesa SET Ativo = 0, DataAlteracao = GETUTCDATE() WHERE Id = @Id", new { Id = id }));
}
