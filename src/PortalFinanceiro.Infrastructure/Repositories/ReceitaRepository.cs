using Dapper;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ReceitaRepository : SqlBaseRepository, IReceitaRepository
{
    public ReceitaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Receita?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Receita>(conn, ReceitaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<Receita>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, string? status = null, Guid? idCategoria = null, string? busca = null)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<Receita>(conn, ReceitaSql.ListarPorMes, new { IdUsuario = idUsuario, Mes = mes, Ano = ano, IdConta = idConta, Status = status, IdCategoria = idCategoria, Busca = busca }));

    public async Task InserirAsync(Receita entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaSql.Inserir, entity));

    public async Task InserirEmMassaAsync(IEnumerable<Receita> entities)
    {
        await ExecuteWithConnectionAsync(async conn =>
        {
            using var tx = conn.BeginTransaction();
            foreach (var entity in entities)
                await conn.ExecuteAsync(ReceitaSql.Inserir, entity, tx);
            tx.Commit();
            return 0;
        });
    }

    public async Task AtualizarAsync(Receita entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaSql.Atualizar, entity));

    public async Task ExcluirAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, $"UPDATE {SqlDialect.Current.SchemaPrefix}Receita SET Ativo = 0, DataAlteracao = GETUTCDATE() WHERE Id = @Id", new { Id = id }));
}
