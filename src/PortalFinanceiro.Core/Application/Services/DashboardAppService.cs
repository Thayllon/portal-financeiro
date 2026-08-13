using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;
using Microsoft.Extensions.Logging;

namespace PortalFinanceiro.Core.Application.Services;

public class DashboardAppService : IDashboardAppService
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly IDespesaRepository _despesaRepository;
    private readonly IRegraReceitaRepository _regraReceitaRepository;
    private readonly IRegraDespesaRepository _regraDespesaRepository;
    private readonly IContaBancariaRepository _contaBancariaRepository;
    private readonly ILogger<DashboardAppService> _logger;

    public DashboardAppService(
        IReceitaRepository receitaRepository,
        IDespesaRepository despesaRepository,
        IRegraReceitaRepository regraReceitaRepository,
        IRegraDespesaRepository regraDespesaRepository,
        IContaBancariaRepository contaBancariaRepository,
        ILogger<DashboardAppService> logger)
    {
        _receitaRepository = receitaRepository;
        _despesaRepository = despesaRepository;
        _regraReceitaRepository = regraReceitaRepository;
        _regraDespesaRepository = regraDespesaRepository;
        _contaBancariaRepository = contaBancariaRepository;
        _logger = logger;
    }

    public async Task<Result<DashboardResponse>> ObterDashboardAsync(Guid idUsuario, int mes, int ano)
    {
        try
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

        var resumoPorCategoria = despesas
            .GroupBy(d => d.Categoria == string.Empty ? "Sem categoria" : d.Categoria)
            .Select(g => new ResumoPorCategoria { Nome = g.Key, Total = g.Sum(d => d.Valor) })
            .OrderByDescending(r => r.Total)
            .ToList();

        var previsao = new List<PrevisaoMensal>();
        for (int i = 1; i <= 3; i++)
        {
            var proximoMes = mes + i;
            var proximoAno = ano;
            if (proximoMes > 12) { proximoMes -= 12; proximoAno++; }

            var rec = regrasReceita
                .Where(r => r.Ativo && r.DataInicio <= new DateTime(proximoAno, proximoMes, 1) && r.DataFim >= new DateTime(proximoAno, proximoMes, 1))
                .Sum(r => r.Valor);
            var desp = regrasDespesa
                .Where(d => d.Ativo && d.DataInicio <= new DateTime(proximoAno, proximoMes, 1) && d.DataFim >= new DateTime(proximoAno, proximoMes, 1))
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
            ResumoPorCategoria = resumoPorCategoria,
            PrevisaoProximosMeses = previsao
        };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dashboard");
            return Erro.Infraestrutura("Erro ao carregar o dashboard.");
        }
    }

    public async Task<Result<DashboardAnualResponse>> ObterDashboardAnualAsync(Guid idUsuario, int ano, Guid? idConta = null)
    {
        try
        {
            var receitasPorMes = (await _receitaRepository.ResumoAnualPorMesAsync(idUsuario, ano, idConta)).ToList();
            var despesasPorMes = (await _despesaRepository.ResumoAnualPorMesAsync(idUsuario, ano, idConta)).ToList();

            var receitasPorConta = (await _receitaRepository.ResumoAnualPorContaAsync(idUsuario, ano)).ToList();
            var despesasPorConta = (await _despesaRepository.ResumoAnualPorContaAsync(idUsuario, ano)).ToList();

            var resumoPorMes = new List<MensalResumoAnual>();
            for (int m = 1; m <= 12; m++)
            {
                var rec = receitasPorMes.FirstOrDefault(r => r.Mes == m);
                var desp = despesasPorMes.FirstOrDefault(d => d.Mes == m);

                var totalRec = rec?.Total ?? 0;
                var totalRecebido = rec?.TotalRealizado ?? 0;
                var totalDesp = desp?.Total ?? 0;
                var totalPago = desp?.TotalRealizado ?? 0;

                resumoPorMes.Add(new MensalResumoAnual
                {
                    Mes = m,
                    TotalReceitas = totalRec,
                    TotalRecebido = totalRecebido,
                    TotalDespesas = totalDesp,
                    TotalPago = totalPago,
                    Saldo = totalRec - totalDesp,
                    SaldoRealizado = totalRecebido - totalPago
                });
            }

            var todasContas = new Dictionary<string, ResumoPorContaAnual>();
            foreach (var rec in receitasPorConta)
            {
                var key = rec.NomeConta;
                if (!todasContas.ContainsKey(key))
                {
                    todasContas[key] = new ResumoPorContaAnual
                    {
                        NomeConta = rec.NomeConta,
                        Banco = rec.Banco,
                        Tipo = rec.Tipo
                    };
                }
                todasContas[key].TotalReceitas = rec.Total;
                todasContas[key].TotalRecebido = rec.TotalRealizado;
            }
            foreach (var desp in despesasPorConta)
            {
                var key = desp.NomeConta;
                if (!todasContas.ContainsKey(key))
                {
                    todasContas[key] = new ResumoPorContaAnual
                    {
                        NomeConta = desp.NomeConta,
                        Banco = desp.Banco,
                        Tipo = desp.Tipo
                    };
                }
                todasContas[key].TotalDespesas = desp.Total;
                todasContas[key].TotalPago = desp.TotalRealizado;
            }

            foreach (var conta in todasContas.Values)
            {
                conta.Saldo = conta.TotalReceitas - conta.TotalDespesas;
                conta.SaldoRealizado = conta.TotalRecebido - conta.TotalPago;
            }

            var totalReceitasAno = resumoPorMes.Sum(m => m.TotalReceitas);
            var totalRecebidoAno = resumoPorMes.Sum(m => m.TotalRecebido);
            var totalDespesasAno = resumoPorMes.Sum(m => m.TotalDespesas);
            var totalPagoAno = resumoPorMes.Sum(m => m.TotalPago);

            return new DashboardAnualResponse
            {
                Ano = ano,
                TotalReceitas = totalReceitasAno,
                TotalRecebido = totalRecebidoAno,
                TotalDespesas = totalDespesasAno,
                TotalPago = totalPagoAno,
                Saldo = totalReceitasAno - totalDespesasAno,
                SaldoRealizado = totalRecebidoAno - totalPagoAno,
                ResumoPorMes = resumoPorMes,
                ResumoPorConta = todasContas.Values.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dashboard anual");
            return Erro.Infraestrutura("Erro ao carregar o dashboard anual.");
        }
    }
}
