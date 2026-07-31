using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class DashboardAppService : IDashboardAppService
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly IDespesaRepository _despesaRepository;
    private readonly IRegraReceitaRepository _regraReceitaRepository;
    private readonly IRegraDespesaRepository _regraDespesaRepository;
    private readonly IContaBancariaRepository _contaBancariaRepository;

    public DashboardAppService(
        IReceitaRepository receitaRepository,
        IDespesaRepository despesaRepository,
        IRegraReceitaRepository regraReceitaRepository,
        IRegraDespesaRepository regraDespesaRepository,
        IContaBancariaRepository contaBancariaRepository)
    {
        _receitaRepository = receitaRepository;
        _despesaRepository = despesaRepository;
        _regraReceitaRepository = regraReceitaRepository;
        _regraDespesaRepository = regraDespesaRepository;
        _contaBancariaRepository = contaBancariaRepository;
    }

    public async Task<Result<DashboardResponse>> ObterDashboardAsync(Guid idUsuario, int mes, int ano)
    {
        var receitas = await _receitaRepository.ListarAsync(idUsuario, mes, ano);
        var despesas = await _despesaRepository.ListarAsync(idUsuario, mes, ano);

        var totalReceitas = receitas.Sum(r => r.Valor);
        var totalRecebido = receitas.Where(r => r.Status == StatusMensal.Realizado).Sum(r => r.Valor);
        var totalDespesas = despesas.Sum(d => d.Valor);
        var totalPago = despesas.Where(d => d.Status == StatusMensal.Realizado).Sum(d => d.Valor);

        var regrasReceita = await _regraReceitaRepository.ListarPorUsuarioAsync(idUsuario);
        var regrasDespesa = await _regraDespesaRepository.ListarPorUsuarioAsync(idUsuario);
        var contas = (await _contaBancariaRepository.ListarPorUsuarioAsync(idUsuario)).Where(c => c.Ativo).ToList();

        var resumoPorConta = new List<ResumoPorConta>();
        foreach (var conta in contas)
        {
            var totalRec = receitas.Where(r => r.IdConta == conta.Id).Sum(r => r.Valor);
            var totalDesp = despesas.Where(d => d.IdConta == conta.Id).Sum(d => d.Valor);

            if (totalRec != 0 || totalDesp != 0)
            {
                resumoPorConta.Add(new ResumoPorConta
                {
                    NomeConta = conta.Nome,
                    Banco = conta.Banco,
                    Tipo = conta.Tipo.ToString(),
                    TotalReceitas = totalRec,
                    TotalDespesas = totalDesp,
                    Saldo = totalRec - totalDesp
                });
            }
        }

        var previsao = new List<PrevisaoMensal>();
        for (int i = 1; i <= 3; i++)
        {
            var proximoMes = mes + i;
            var proximoAno = ano;
            if (proximoMes > 12) { proximoMes -= 12; proximoAno++; }

            var rec = regrasReceita
                .Where(r => r.Ativo && r.DataInicio <= new DateTime(proximoAno, proximoMes, 1))
                .Sum(r => r.Valor);
            var desp = regrasDespesa
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
            ResumoPorConta = resumoPorConta,
            PrevisaoProximosMeses = previsao
        };
    }
}
