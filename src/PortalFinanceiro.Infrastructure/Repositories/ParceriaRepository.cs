using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ParceriaRepository : SqlBaseRepository, IParceriaRepository
{
    public ParceriaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Parceria?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Parceria>(conn, ParceriaSql.ObterPorId, new { Id = id }));

    public async Task<ParceriaProjecao?> ObterProjecaoPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ParceriaProjecao>(conn, ParceriaSql.ObterProjecaoPorId, new { Id = id }));

    public async Task<IEnumerable<ParceriaProjecao>> ListarAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ParceriaProjecao>(conn, ParceriaSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(Parceria entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ParceriaSql.Inserir, entity));

    public async Task AtualizarAsync(Parceria entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ParceriaSql.Atualizar, entity));

    public async Task<decimal> SomarReceitasPorStatusAsync(Guid idParceria, int status)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarReceitas, new { IdParceria = idParceria, Status = status }));

    public async Task<decimal> SomarDespesasPorStatusAsync(Guid idParceria, int status)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarDespesas, new { IdParceria = idParceria, Status = status }));
}
