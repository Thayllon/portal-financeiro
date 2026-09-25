using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;
using System.Data;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ParceriaRepository : SqlBaseRepository, IParceriaRepository
{
    public ParceriaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Parceria?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Parceria>(conn, ParceriaSql.ObterPorId, new { Id = id }));

    public async Task<ParceriaProjecao?> ObterProjecaoPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ParceriaProjecao>(conn, ParceriaSql.ObterProjecaoPorId, new { Id = id }));

    public async Task<IEnumerable<ParceriaProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ParceriaProjecao>(conn, ParceriaSql.ListarPorUsuario, new { IdUsuario = idUsuario, Ativo = ativo }));

    public async Task<IEnumerable<ParceriaProjecao>> ListarComTotaisAsync(Guid idUsuario, bool? ativo, int statusRealizado)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ParceriaProjecao>(conn, ParceriaSql.ListarPorUsuarioComTotais, new { IdUsuario = idUsuario, Ativo = ativo, StatusRealizado = statusRealizado }));

    public async Task InserirAsync(Parceria entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ParceriaSql.Inserir, entity));

    public async Task AtualizarAsync(Parceria entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ParceriaSql.Atualizar, entity));

    public async Task<decimal> SomarReceitasPorStatusAsync(Guid idParceria, int status)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarReceitas, new { IdParceria = idParceria, Status = status }));

    public async Task<decimal> SomarDespesasPorStatusAsync(Guid idParceria, int status)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarDespesas, new { IdParceria = idParceria, Status = status }));

    public async Task<ResumoParceriaAnual> ResumoAnualAsync(Guid idUsuario, int ano, Guid? idConta = null)
        => await ResumoParceriaAsync(
            (conn, status) => QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarReceitasAnual, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta, Status = status }),
            (conn, status) => QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarDespesasAnual, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta, Status = status }),
            conn => QueryFirstOrDefaultAsync<int>(conn, ParceriaSql.ContarParceriasAnual, new { IdUsuario = idUsuario, Ano = ano, IdConta = idConta }));

    public async Task<ResumoParceriaAnual> ResumoMensalAsync(Guid idUsuario, int ano, int mes, Guid? idConta = null)
        => await ResumoParceriaAsync(
            (conn, status) => QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarReceitasMensal, new { IdUsuario = idUsuario, Ano = ano, Mes = mes, IdConta = idConta, Status = status }),
            (conn, status) => QueryFirstOrDefaultAsync<decimal>(conn, ParceriaSql.SomarDespesasMensal, new { IdUsuario = idUsuario, Ano = ano, Mes = mes, IdConta = idConta, Status = status }),
            conn => QueryFirstOrDefaultAsync<int>(conn, ParceriaSql.ContarParceriasMensal, new { IdUsuario = idUsuario, Ano = ano, Mes = mes, IdConta = idConta }));

    private async Task<ResumoParceriaAnual> ResumoParceriaAsync(
        Func<IDbConnection, int, Task<decimal>> somarReceitas,
        Func<IDbConnection, int, Task<decimal>> somarDespesas,
        Func<IDbConnection, Task<int>> contar)
        => await ExecuteWithConnectionAsync(async conn =>
        {
            var recebido = await somarReceitas(conn, 2);
            var aReceber = await somarReceitas(conn, 1);
            var pago = await somarDespesas(conn, 2);
            var aPagar = await somarDespesas(conn, 1);
            var qtd = await contar(conn);
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
