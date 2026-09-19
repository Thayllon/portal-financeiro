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

    public async Task<ResumoParceriaAnual> ResumoAnualAsync(Guid idUsuario, int ano, Guid? idConta = null)
        => await ExecuteWithConnectionAsync(async conn =>
        {
            var recebido = await QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarReceitasAnual, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta, Status = 2 });
            var aReceber = await QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarReceitasAnual, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta, Status = 1 });
            var pago = await QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarDespesasAnual, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta, Status = 2 });
            var aPagar = await QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarDespesasAnual, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta, Status = 1 });
            var qtd = await QueryFirstOrDefaultAsync<int>(conn, ParceriaSql.ContarParceriasAnual, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta });
            return new ResumoParceriaAnual
            {
                TotalRecebido = recebido,
                TotalPago = pago,
                AReceber = aReceber,
                APagar = aPagar,
                QtdParcerias = qtd
            };
        });
}
