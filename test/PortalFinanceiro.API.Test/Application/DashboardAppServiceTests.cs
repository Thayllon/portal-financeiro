using FluentAssertions;
using Microsoft.Extensions.Logging;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.API.Test;

public class DashboardAppServiceTests
{
    private sealed class ReceitaRepositoryFake : IReceitaRepository
    {
        public List<ResumoAnualItem> PorMes { get; } = new();

        public Task<Receita?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null) => throw new NotImplementedException();
        public Task<int> ContarPorCategoriaAsync(Guid idCategoria) => throw new NotImplementedException();
        public Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria) => throw new NotImplementedException();
        public Task<int> ContarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Receita>> ListarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarPorContratoAsync(Guid idContrato) => throw new NotImplementedException();
        public Task InserirAsync(Receita entity) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<Receita> entities) => throw new NotImplementedException();
        public Task AtualizarAsync(Receita entity) => throw new NotImplementedException();
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null)
            => Task.FromResult<IEnumerable<ResumoAnualItem>>(PorMes.Where(r => ano == 2020));
        public Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null)
            => Task.FromResult<IEnumerable<ResumoAnualContaItem>>(Array.Empty<ResumoAnualContaItem>());
        public Task<IEnumerable<ResumoAnualCategoriaItem>> ResumoAnualPorCategoriaAsync(Guid idUsuario, int ano, Guid? idConta = null)
            => Task.FromResult<IEnumerable<ResumoAnualCategoriaItem>>(Array.Empty<ResumoAnualCategoriaItem>());
    }

    private sealed class DespesaRepositoryFake : IDespesaRepository
    {
        public List<ResumoAnualItem> PorMes { get; } = new();

        public Task<Despesa?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<DespesaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<DespesaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null) => throw new NotImplementedException();
        public Task<int> ContarPorCategoriaAsync(Guid idCategoria) => throw new NotImplementedException();
        public Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria) => throw new NotImplementedException();
        public Task<int> ContarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Despesa>> ListarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Despesa>> ListarPorReceitaOrigemAsync(Guid idReceitaOrigem) => throw new NotImplementedException();
        public Task<IEnumerable<DespesaProjecao>> ListarPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task InserirAsync(Despesa entity) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<Despesa> entities) => throw new NotImplementedException();
        public Task AtualizarAsync(Despesa entity) => throw new NotImplementedException();
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null)
            => Task.FromResult<IEnumerable<ResumoAnualItem>>(PorMes.Where(r => ano == 2020));
        public Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null)
            => Task.FromResult<IEnumerable<ResumoAnualContaItem>>(Array.Empty<ResumoAnualContaItem>());
        public Task<IEnumerable<ResumoAnualCategoriaItem>> ResumoAnualPorCategoriaAsync(Guid idUsuario, int ano, Guid? idConta = null)
            => Task.FromResult<IEnumerable<ResumoAnualCategoriaItem>>(Array.Empty<ResumoAnualCategoriaItem>());
    }

    private sealed class RegraReceitaRepositoryFake : IRegraReceitaRepository
    {
        public Task<RegraReceita?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<RegraReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<RegraReceitaProjecao>> ListarPorUsuarioAsync(Guid idUsuario)
            => Task.FromResult<IEnumerable<RegraReceitaProjecao>>(Array.Empty<RegraReceitaProjecao>());
        public Task InserirAsync(RegraReceita entity) => throw new NotImplementedException();
        public Task AtualizarAsync(RegraReceita entity) => throw new NotImplementedException();
    }

    private sealed class RegraDespesaRepositoryFake : IRegraDespesaRepository
    {
        public Task<RegraDespesa?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<RegraDespesaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<RegraDespesaProjecao>> ListarPorUsuarioAsync(Guid idUsuario)
            => Task.FromResult<IEnumerable<RegraDespesaProjecao>>(Array.Empty<RegraDespesaProjecao>());
        public Task InserirAsync(RegraDespesa entity) => throw new NotImplementedException();
        public Task AtualizarAsync(RegraDespesa entity) => throw new NotImplementedException();
    }

    private sealed class ContaBancariaRepositoryFake : IContaBancariaRepository
    {
        public Task<ContaBancaria?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ContaBancaria>> ListarPorUsuarioAsync(Guid idUsuario)
            => Task.FromResult<IEnumerable<ContaBancaria>>(Array.Empty<ContaBancaria>());
        public Task<int> ContarReceitasAsync(Guid idConta) => throw new NotImplementedException();
        public Task<int> ContarDespesasAsync(Guid idConta) => throw new NotImplementedException();
        public Task InserirAsync(ContaBancaria entity) => throw new NotImplementedException();
        public Task AtualizarAsync(ContaBancaria entity) => throw new NotImplementedException();
        public Task LimparPadraoAsync(Guid idUsuario) => throw new NotImplementedException();
    }

    private sealed class ParceriaRepositoryFake : IParceriaRepository
    {
        public Task<Parceria?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ParceriaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ParceriaProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null) => throw new NotImplementedException();
        public Task InserirAsync(Parceria entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Parceria entity) => throw new NotImplementedException();
        public Task<decimal> SomarReceitasPorStatusAsync(Guid idParceria, int status) => throw new NotImplementedException();
        public Task<decimal> SomarDespesasPorStatusAsync(Guid idParceria, int status) => throw new NotImplementedException();
        public Task<ResumoParceriaAnual> ResumoAnualAsync(Guid idUsuario, int ano, Guid? idConta = null)
            => Task.FromResult(new ResumoParceriaAnual());
        public Task<ResumoParceriaAnual> ResumoMensalAsync(Guid idUsuario, int ano, int mes, Guid? idConta = null) => throw new NotImplementedException();
    }

    private sealed class LoggerFake : ILogger<DashboardAppService>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    [Fact]
    public async Task ObterDashboardAnual_CalculaSaldoAcumuladoMesAMes()
    {
        var receitas = new ReceitaRepositoryFake();
        receitas.PorMes.Add(new ResumoAnualItem { Mes = 1, Total = 3000, TotalRealizado = 3000 });
        receitas.PorMes.Add(new ResumoAnualItem { Mes = 2, Total = 1000, TotalRealizado = 1000 });

        var despesas = new DespesaRepositoryFake();
        despesas.PorMes.Add(new ResumoAnualItem { Mes = 1, Total = 1000, TotalRealizado = 1000 });
        despesas.PorMes.Add(new ResumoAnualItem { Mes = 2, Total = 1500, TotalRealizado = 1500 });

        var service = new DashboardAppService(
            receitas,
            despesas,
            new RegraReceitaRepositoryFake(),
            new RegraDespesaRepositoryFake(),
            new ContaBancariaRepositoryFake(),
            new ParceriaRepositoryFake(),
            new LoggerFake());

        var result = await service.ObterDashboardAnualAsync(Guid.NewGuid(), 2020);

        result.EhSucesso.Should().BeTrue();
        var meses = result.Dado!.ResumoPorMes;
        meses[0].Saldo.Should().Be(2000);
        meses[0].SaldoAcumulado.Should().Be(2000);
        meses[1].Saldo.Should().Be(-500);
        meses[1].SaldoAcumulado.Should().Be(1500);
    }
}
