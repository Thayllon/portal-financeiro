using Dapper;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ReceitaRepository : SqlBaseRepository, IReceitaRepository
{
    public ReceitaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Receita?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Receita>(conn, ReceitaSql.ObterPorId, new { Id = id }));

    public async Task<ReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ReceitaProjecao>(conn, ReceitaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<ReceitaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ReceitaProjecao>(conn, ReceitaSql.ListarPorMes, new { IdUsuario = idUsuario, Mes = mes, Ano = ano, IdConta = idConta, Status = status, IdCategoria = idCategoria, Busca = busca }));

    public async Task<int> ContarPorCategoriaAsync(Guid idCategoria)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ReceitaSql.ContarPorCategoria, new { IdCategoria = idCategoria }));

    public async Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ReceitaSql.ContarPorSubcategoria, new { IdSubcategoria = idSubcategoria }));

    public async Task<int> ContarPorRegraAsync(Guid idRegra)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ReceitaSql.ContarPorRegra, new { IdRegra = idRegra }));

    public async Task<IEnumerable<Receita>> ListarPorRegraAsync(Guid idRegra)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<Receita>(conn, ReceitaSql.ListarPorRegra, new { IdRegra = idRegra }));

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
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaSql.Excluir, new { Id = id }));

    public async Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ResumoAnualItem>(conn, ReceitaSql.ResumoAnualPorMes, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta }));

    public async Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ResumoAnualContaItem>(conn, ReceitaSql.ResumoAnualPorConta, new { IdUsuario = idUsuario, Ano = ano }));
}
