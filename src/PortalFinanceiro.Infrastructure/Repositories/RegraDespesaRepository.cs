using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class RegraDespesaRepository : SqlBaseRepository, IRegraDespesaRepository
{
    public RegraDespesaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<RegraDespesa?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<RegraDespesa>(conn, RegraDespesaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<RegraDespesa>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<RegraDespesa>(conn, RegraDespesaSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(RegraDespesa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, RegraDespesaSql.Inserir, entity));

    public async Task AtualizarAsync(RegraDespesa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, RegraDespesaSql.Atualizar, entity));
}
