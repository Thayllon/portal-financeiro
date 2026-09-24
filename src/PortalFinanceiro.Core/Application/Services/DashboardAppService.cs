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
    private readonly IParceriaRepository _parceriaRepository;
    private readonly ILogger<DashboardAppService> _logger;

    public DashboardAppService(
        IReceitaRepository receitaRepository,
        IDespesaRepository despesaRepository,
        IRegraReceitaRepository regraReceitaRepository,
        IRegraDespesaRepository regraDespesaRepository,
        IContaBancariaRepository contaBancariaRepository,
        IParceriaRepository parceriaRepository,
        ILogger<DashboardAppService> logger)
    {
        _receitaRepository = receitaRepository;
        _despesaRepository = despesaRepository;
        _regraReceitaRepository = regraReceitaRepository;
        _regraDespesaRepository = regraDespesaRepository;
        _contaBancariaRepository = contaBancariaRepository;
        _parceriaRepository = parceriaRepository;
        _logger = logger;
    }

    public async Task<Result<DashboardResponse>> ObterDashboardAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null)
    {
        try
        {
            var receitas = await _receitaRepository.ListarAsync(idUsuario, mes, ano, idConta);
            var despesas = await _despesaRepository.ListarAsync(idUsuario, mes, ano, idConta);

        var totalReceitas = receitas.Sum(r => r.Valor);
        var totalRecebido = receitas.Where(r => r.Status == StatusMensal.Realizado).Sum(r => r.Valor);
        var totalDespesas = despesas.Sum(d => d.Valor);
        var totalPago = despesas.Where(d => d.Status == StatusMensal.Realizado).Sum(d => d.Valor);

        var regrasReceita = (await _regraReceitaRepository.ListarPorUsuarioAsync(idUsuario)).Where(r => idConta == null || r.IdConta == idConta).ToList();
        var regrasDespesa = (await _regraDespesaRepository.ListarPorUsuarioAsync(idUsuario)).Where(d => idConta == null || d.IdConta == idConta).ToList();
        var contas = (await _contaBancariaRepository.ListarPorUsuarioAsync(idUsuario)).Where(c => c.Ativo && (idConta == null || c.Id == idConta)).ToList();

        var inicioMesAtual = new DateTime(ano, mes, 1);
        var inicioMesSeguinteAtual = inicioMesAtual.AddMonths(1);

        var resumoPorConta = new List<ResumoPorConta>();
        foreach (var conta in contas)
        {
            var totalRec = receitas.Where(r => r.IdConta == conta.Id).Sum(r => r.Valor);
            var totalDesp = despesas.Where(d => d.IdConta == conta.Id).Sum(d => d.Valor);
            var recPrevisto = regrasReceita
                .Where(r => r.Ativo && r.IdConta == conta.Id && r.DataInicio < inicioMesSeguinteAtual && r.DataFim >= inicioMesAtual)
                .Sum(r => Math.Max(0, r.Valor - receitas.Where(l => l.IdRegra == r.Id && l.IdConta == conta.Id).Sum(l => l.Valor)));
            var despPrevisto = regrasDespesa
                .Where(d => d.Ativo && d.IdConta == conta.Id && d.DataInicio < inicioMesSeguinteAtual && d.DataFim >= inicioMesAtual)
                .Sum(d => Math.Max(0, d.Valor - despesas.Where(l => l.IdRegra == d.Id && l.IdConta == conta.Id).Sum(l => l.Valor)));

            resumoPorConta.Add(new ResumoPorConta
            {
                NomeConta = conta.Nome,
                Banco = conta.Banco,
                Tipo = conta.Tipo.ToString(),
                TotalReceitas = totalRec,
                TotalDespesas = totalDesp,
                Saldo = totalRec - totalDesp,
                TotalReceitasPrevisto = recPrevisto,
                TotalDespesasPrevisto = despPrevisto
            });
        }

        var distribuicaoReceitas = MontarDistribuicao(receitas.Select(r => new Domain.Projections.ResumoAnualCategoriaItem { Categoria = r.Categoria, Subcategoria = r.Subcategoria, Total = r.Valor }));
        var distribuicaoDespesas = MontarDistribuicao(despesas.Select(d => new Domain.Projections.ResumoAnualCategoriaItem { Categoria = d.Categoria, Subcategoria = d.Subcategoria, Total = d.Valor }));

        var previsao = new List<PrevisaoMensal>();
        for (int i = 1; i <= 3; i++)
        {
            var proximoMes = mes + i;
            var proximoAno = ano;
            if (proximoMes > 12) { proximoMes -= 12; proximoAno++; }

            var inicioMes = new DateTime(proximoAno, proximoMes, 1);
            var inicioMesSeguinte = inicioMes.AddMonths(1);

            var rec = regrasReceita
                .Where(r => r.Ativo && r.DataInicio < inicioMesSeguinte && r.DataFim >= inicioMes)
                .Sum(r => r.Valor);
            var desp = regrasDespesa
                .Where(d => d.Ativo && d.DataInicio < inicioMesSeguinte && d.DataFim >= inicioMes)
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

        var totalReceitasPrevisto = regrasReceita
            .Where(r => r.Ativo && r.DataInicio < inicioMesSeguinteAtual && r.DataFim >= inicioMesAtual)
            .Sum(r => Math.Max(0, r.Valor - receitas.Where(l => l.IdRegra == r.Id).Sum(l => l.Valor)));
        var totalDespesasPrevisto = regrasDespesa
            .Where(d => d.Ativo && d.DataInicio < inicioMesSeguinteAtual && d.DataFim >= inicioMesAtual)
            .Sum(d => Math.Max(0, d.Valor - despesas.Where(l => l.IdRegra == d.Id).Sum(l => l.Valor)));

        var totalReceitasRecorrentes = receitas.Where(r => r.EhRecorrente).Sum(r => r.Valor);
        var totalDespesasRecorrentes = despesas.Where(d => d.EhRecorrente).Sum(d => d.Valor);

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
            TotalReceitasPrevisto = totalReceitasPrevisto,
            TotalDespesasPrevisto = totalDespesasPrevisto,
            TotalReceitasRecorrentes = totalReceitasRecorrentes,
            TotalDespesasRecorrentes = totalDespesasRecorrentes,
            SaldoPrevisto = totalReceitas + totalReceitasPrevisto - totalDespesas - totalDespesasPrevisto,
            ResumoPorConta = resumoPorConta,
            DistribuicaoReceitas = distribuicaoReceitas,
            DistribuicaoDespesas = distribuicaoDespesas,
            PrevisaoProximosMeses = previsao,
            ResumoParcerias = await MapearResumoParceriasMensalAsync(idUsuario, ano, mes, idConta)
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

            var receitasPorConta = (await _receitaRepository.ResumoAnualPorContaAsync(idUsuario, ano, idConta)).ToList();
            var despesasPorConta = (await _despesaRepository.ResumoAnualPorContaAsync(idUsuario, ano, idConta)).ToList();

            var receitasPorCategoria = (await _receitaRepository.ResumoAnualPorCategoriaAsync(idUsuario, ano, idConta)).ToList();
            var despesasPorCategoria = (await _despesaRepository.ResumoAnualPorCategoriaAsync(idUsuario, ano, idConta)).ToList();

            var resumoParceria = await _parceriaRepository.ResumoAnualAsync(idUsuario, ano, idConta);

            var resumoPorMes = new List<MensalResumoAnual>();
            var saldoAcumulado = 0m;
            for (int m = 1; m <= 12; m++)
            {
                var rec = receitasPorMes.FirstOrDefault(r => r.Mes == m);
                var desp = despesasPorMes.FirstOrDefault(d => d.Mes == m);

                var totalRec = rec?.Total ?? 0;
                var totalRecebido = rec?.TotalRealizado ?? 0;
                var totalDesp = desp?.Total ?? 0;
                var totalPago = desp?.TotalRealizado ?? 0;
                var saldo = totalRec - totalDesp;
                saldoAcumulado += saldo;

                resumoPorMes.Add(new MensalResumoAnual
                {
                    Mes = m,
                    TotalReceitas = totalRec,
                    TotalRecebido = totalRecebido,
                    TotalDespesas = totalDesp,
                    TotalPago = totalPago,
                    Saldo = saldo,
                    SaldoRealizado = totalRecebido - totalPago,
                    SaldoAcumulado = saldoAcumulado
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
            var saldoAno = totalReceitasAno - totalDespesasAno;

            var anoAnterior = ano - 1;
            var recAnterior = (await _receitaRepository.ResumoAnualPorMesAsync(idUsuario, anoAnterior, idConta)).Sum(r => r.Total);
            var despAnterior = (await _despesaRepository.ResumoAnualPorMesAsync(idUsuario, anoAnterior, idConta)).Sum(d => d.Total);

            var hoje = DateTime.Today;
            var mesesConsiderados = ano < hoje.Year ? 12 : ano == hoje.Year ? hoje.Month : 0;
            var mediaMensalSaldo = mesesConsiderados > 0 ? Math.Round(saldoAno / mesesConsiderados, 2) : 0;

            var previsaoRestante = await MontarPrevisaoRestanteAnoAsync(idUsuario, ano);

            return new DashboardAnualResponse
            {
                Ano = ano,
                TotalReceitas = totalReceitasAno,
                TotalRecebido = totalRecebidoAno,
                TotalDespesas = totalDespesasAno,
                TotalPago = totalPagoAno,
                Saldo = saldoAno,
                SaldoRealizado = totalRecebidoAno - totalPagoAno,
                VariacaoReceitasPercentual = CalcularVariacao(totalReceitasAno, recAnterior),
                VariacaoDespesasPercentual = CalcularVariacao(totalDespesasAno, despAnterior),
                VariacaoSaldoPercentual = CalcularVariacao(saldoAno, recAnterior - despAnterior),
                MediaMensalSaldo = mediaMensalSaldo,
                MesesConsiderados = mesesConsiderados,
                ResumoPorMes = resumoPorMes,
                ResumoPorConta = todasContas.Values.ToList(),
                DistribuicaoReceitas = MontarDistribuicao(receitasPorCategoria),
                DistribuicaoDespesas = MontarDistribuicao(despesasPorCategoria),
                PrevisaoRestanteAno = previsaoRestante,
                ResumoParcerias = new ResumoParceriasAnual
                {
                    TotalRecebido = resumoParceria.TotalRecebido,
                    TotalPago = resumoParceria.TotalPago,
                    AReceber = resumoParceria.AReceber,
                    APagar = resumoParceria.APagar,
                    QtdParcerias = resumoParceria.QtdParcerias
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dashboard anual");
            return Erro.Infraestrutura("Erro ao carregar o dashboard anual.");
        }
    }

    private async Task<ResumoParceriasAnual> MapearResumoParceriasMensalAsync(Guid idUsuario, int ano, int mes, Guid? idConta)
    {
        var resumo = await _parceriaRepository.ResumoMensalAsync(idUsuario, ano, mes, idConta);
        return new ResumoParceriasAnual
        {
            TotalRecebido = resumo.TotalRecebido,
            TotalPago = resumo.TotalPago,
            AReceber = resumo.AReceber,
            APagar = resumo.APagar,
            QtdParcerias = resumo.QtdParcerias
        };
    }

    private static decimal? CalcularVariacao(decimal atual, decimal anterior)    {
        if (anterior == 0)
            return null;
        return Math.Round((atual - anterior) / Math.Abs(anterior) * 100, 1);
    }

    private static string ChaveCategoria(string? nome) =>
        string.IsNullOrWhiteSpace(nome) ? "Sem categoria" : nome.Trim();

    private static List<DistribuicaoCategoriaAnual> MontarDistribuicao(IEnumerable<Domain.Projections.ResumoAnualCategoriaItem> itens)
    {
        var lista = itens.ToList();
        var totalGeral = lista.Sum(i => i.Total);
        var grupos = lista
            .GroupBy(i => ChaveCategoria(i.Categoria), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var totalCategoria = g.Sum(i => i.Total);
                return new DistribuicaoCategoriaAnual
                {
                    Nome = g.Key,
                    Total = totalCategoria,
                    Percentual = totalGeral == 0 ? 0 : Math.Round(totalCategoria / totalGeral * 100, 1),
                    Subcategorias = g
                        .Where(i => !string.IsNullOrWhiteSpace(i.Subcategoria))
                        .GroupBy(i => i.Subcategoria!.Trim(), StringComparer.OrdinalIgnoreCase)
                        .Select(sg =>
                        {
                            var totalSub = sg.Sum(i => i.Total);
                            return new DistribuicaoCategoriaAnual
                            {
                                Nome = sg.Key,
                                Total = totalSub,
                                Percentual = totalGeral == 0 ? 0 : Math.Round(totalSub / totalGeral * 100, 1)
                            };
                        })
                        .OrderByDescending(s => s.Total)
                        .ToList()
                };
            })
            .OrderByDescending(c => c.Total)
            .ToList();
        return grupos;
    }

    private async Task<List<PrevisaoMensal>> MontarPrevisaoRestanteAnoAsync(Guid idUsuario, int ano)
    {
        var hoje = DateTime.Today;
        int mesInicial;
        if (ano < hoje.Year)
            return [];
        mesInicial = ano == hoje.Year ? hoje.Month : 1;

        var regrasReceita = (await _regraReceitaRepository.ListarPorUsuarioAsync(idUsuario)).ToList();
        var regrasDespesa = (await _regraDespesaRepository.ListarPorUsuarioAsync(idUsuario)).ToList();

        var previsao = new List<PrevisaoMensal>();
        for (int m = mesInicial; m <= 12; m++)
        {
            var inicioMes = new DateTime(ano, m, 1);
            var inicioMesSeguinte = inicioMes.AddMonths(1);

            var rec = regrasReceita
                .Where(r => r.Ativo && r.DataInicio < inicioMesSeguinte && r.DataFim >= inicioMes)
                .Sum(r => r.Valor);
            var desp = regrasDespesa
                .Where(d => d.Ativo && d.DataInicio < inicioMesSeguinte && d.DataFim >= inicioMes)
                .Sum(d => d.Valor);

            previsao.Add(new PrevisaoMensal
            {
                Mes = m,
                Ano = ano,
                TotalReceitas = rec,
                TotalDespesas = desp,
                SaldoPrevisto = rec - desp
            });
        }
        return previsao;
    }
}
