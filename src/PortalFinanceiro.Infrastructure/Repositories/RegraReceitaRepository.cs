using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class RegraReceitaRepository : SqlBaseRepository, IRegraReceitaRepository
{
    public RegraReceitaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<RegraReceita?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<RegraReceita>(conn, RegraReceitaSql.ObterPorId, new { Id = id }));

    public async Task<RegraReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<RegraReceitaProjecao>(conn, RegraReceitaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<RegraReceitaProjecao>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<RegraReceitaProjecao>(conn, RegraReceitaSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(RegraReceita entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, RegraReceitaSql.Inserir, entity));

    public async Task AtualizarAsync(RegraReceita entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, RegraReceitaSql.Atualizar, entity));
}
