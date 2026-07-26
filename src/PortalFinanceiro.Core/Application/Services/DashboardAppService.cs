using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class DashboardAppService : IDashboardAppService
{
    private readonly IReceitaMensalRepository _receitaMensalRepository;
    private readonly IDespesaMensalRepository _despesaMensalRepository;
    private readonly IReceitaRecorrenteRepository _receitaRecorrenteRepository;
    private readonly IDespesaRecorrenteRepository _despesaRecorrenteRepository;

    public DashboardAppService(
        IReceitaMensalRepository receitaMensalRepository,
        IDespesaMensalRepository despesaMensalRepository,
        IReceitaRecorrenteRepository receitaRecorrenteRepository,
        IDespesaRecorrenteRepository despesaRecorrenteRepository)
    {
        _receitaMensalRepository = receitaMensalRepository;
        _despesaMensalRepository = despesaMensalRepository;
        _receitaRecorrenteRepository = receitaRecorrenteRepository;
        _despesaRecorrenteRepository = despesaRecorrenteRepository;
    }

    public async Task<Result<DashboardResponse>> ObterDashboardAsync(Guid idUsuario, int mes, int ano)
    {
        var receitas = await _receitaMensalRepository.ListarPorMesAsync(idUsuario, mes, ano);
        var despesas = await _despesaMensalRepository.ListarPorMesAsync(idUsuario, mes, ano);

        var totalReceitas = receitas.Sum(r => r.Valor);
        var totalRecebido = receitas.Where(r => r.Status == StatusMensal.Realizado).Sum(r => r.Valor);
        var totalDespesas = despesas.Sum(d => d.Valor);
        var totalPago = despesas.Where(d => d.Status == StatusMensal.Realizado).Sum(d => d.Valor);

        var recorrentesReceita = await _receitaRecorrenteRepository.ListarPorUsuarioAsync(idUsuario);
        var recorrentesDespesa = await _despesaRecorrenteRepository.ListarPorUsuarioAsync(idUsuario);

        var previsao = new List<PrevisaoMensal>();
        for (int i = 1; i <= 3; i++)
        {
            var proximoMes = mes + i;
            var proximoAno = ano;
            if (proximoMes > 12)
            {
                proximoMes -= 12;
                proximoAno++;
            }

            var rec = recorrentesReceita
                .Where(r => r.Ativo && r.DataInicio <= new DateTime(proximoAno, proximoMes, 1))
                .Sum(r => r.Valor);

            var desp = recorrentesDespesa
                .Where(d => d.Ativo && d.DataInicio <= new DateTime(proximoAno, proximoMes, 1))
                .Sum(d => d.Valor);

            previsao.Add(new PrevisaoMensal
            {
                Mes = proximoMes,
                Ano = proximoAno,
                TotalReceitas = rec,
                TotalDespesas = desp,
                SaldoPrevisto = rec - desp
            });
        }

        return new DashboardResponse
        {
            Mes = mes,
            Ano = ano,
            TotalReceitas = totalReceitas,
            TotalRecebido = totalRecebido,
            TotalDespesas = totalDespesas,
            TotalPago = totalPago,
            Saldo = totalReceitas - totalDespesas,
            SaldoRealizado = totalRecebido - totalPago,
            PrevisaoProximosMeses = previsao
        };
    }
}
