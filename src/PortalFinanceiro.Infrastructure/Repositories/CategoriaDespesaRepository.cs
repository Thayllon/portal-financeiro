using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class CategoriaDespesaRepository : SqlBaseRepository, ICategoriaDespesaRepository
{
    public CategoriaDespesaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<CategoriaDespesa?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<CategoriaDespesa>(conn, CategoriaDespesaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<CategoriaDespesa>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<CategoriaDespesa>(conn, CategoriaDespesaSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task<IEnumerable<CategoriaDespesa>> ListarPorPaiAsync(Guid? categoriaPaiId)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<CategoriaDespesa>(conn, CategoriaDespesaSql.ListarPorPai, new { CategoriaPaiId = categoriaPaiId }));

    public async Task InserirAsync(CategoriaDespesa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaDespesaSql.Inserir, entity));

    public async Task AtualizarAsync(CategoriaDespesa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaDespesaSql.Atualizar, entity));

    public async Task ExcluirAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaDespesaSql.Excluir, new { Id = id }));
}
