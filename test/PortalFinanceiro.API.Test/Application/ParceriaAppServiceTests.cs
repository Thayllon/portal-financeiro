using FluentAssertions;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.API.Test;

public class ParceriaAppServiceTests
{
    private sealed class ParceriaRepositoryFake : IParceriaRepository
    {
        public List<ParceriaProjecao> Itens { get; } = new();
        public ParceriaProjecao? PorId { get; set; }
        public int ChamadasSomarReceitas { get; private set; }
        public int ChamadasSomarDespesas { get; private set; }
        public decimal SomaReceitas { get; set; }
        public decimal SomaDespesas { get; set; }

        public Task<Parceria?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ParceriaProjecao?> ObterProjecaoPorIdAsync(Guid id) => Task.FromResult(PorId);
        public Task<IEnumerable<ParceriaProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null) => throw new NotImplementedException();
        public Task<IEnumerable<ParceriaProjecao>> ListarComTotaisAsync(Guid idUsuario, bool? ativo, int statusRealizado)
            => Task.FromResult<IEnumerable<ParceriaProjecao>>(Itens.ToList());
        public Task InserirAsync(Parceria entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Parceria entity) => throw new NotImplementedException();
        public Task<decimal> SomarReceitasPorStatusAsync(Guid idParceria, int status)
        {
            ChamadasSomarReceitas++;
            return Task.FromResult(SomaReceitas);
        }
        public Task<decimal> SomarDespesasPorStatusAsync(Guid idParceria, int status)
        {
            ChamadasSomarDespesas++;
            return Task.FromResult(SomaDespesas);
        }
        public Task<ResumoParceriaAnual> ResumoAnualAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<ResumoParceriaAnual> ResumoMensalAsync(Guid idUsuario, int ano, int mes, Guid? idConta = null) => throw new NotImplementedException();
    }

    private sealed class PessoaRepositoryFake : IPessoaRepository
    {
        public Task<Pessoa?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<Pessoa>> ListarPorUsuarioAsync(Guid idUsuario) => throw new NotImplementedException();
        public Task InserirAsync(Pessoa entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Pessoa entity) => throw new NotImplementedException();
    }

    private static ParceriaProjecao NovaProjecao(Guid usuario, decimal totalRecebido = 0, decimal totalPago = 0)
        => new()
        {
            Id = Guid.NewGuid(),
            IdUsuario = usuario,
            Nome = "Parceria",
            IdParceiro = Guid.NewGuid(),
            Parceiro = "Parceiro",
            IdCliente = Guid.NewGuid(),
            Cliente = "Cliente",
            Valor = 10000,
            PercentualParceiro = 30,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            TotalRecebido = totalRecebido,
            TotalPago = totalPago
        };

    [Fact]
    public async Task Listar_UsaTotaisDaProjecao_SemQueriesPorItem()
    {
        var repos = new ParceriaRepositoryFake();
        var usuario = Guid.NewGuid();
        repos.Itens.Add(NovaProjecao(usuario, totalRecebido: 4000, totalPago: 1000));
        repos.Itens.Add(NovaProjecao(usuario, totalRecebido: 2000, totalPago: 500));
        var service = new ParceriaAppService(repos, new PessoaRepositoryFake());

        var result = await service.ListarAsync(usuario);

        result.EhSucesso.Should().BeTrue();
        var lista = result.Dado!.ToList();
        lista.Should().HaveCount(2);
        lista[0].TotalRecebido.Should().Be(4000);
        lista[0].TotalPago.Should().Be(1000);
        lista[0].ValorParceiro.Should().Be(3000);
        lista[0].MinhaParte.Should().Be(7000);
        lista[0].FaltaReceber.Should().Be(6000);
        lista[0].FaltaPagar.Should().Be(2000);
        repos.ChamadasSomarReceitas.Should().Be(0);
        repos.ChamadasSomarDespesas.Should().Be(0);
    }

    [Fact]
    public async Task ObterPorId_BuscaTotaisIndividualmente()
    {
        var repos = new ParceriaRepositoryFake();
        var usuario = Guid.NewGuid();
        repos.PorId = NovaProjecao(usuario);
        repos.SomaReceitas = 4000;
        repos.SomaDespesas = 1000;
        var service = new ParceriaAppService(repos, new PessoaRepositoryFake());

        var result = await service.ObterPorIdAsync(repos.PorId.Id, usuario);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.TotalRecebido.Should().Be(4000);
        result.Dado!.TotalPago.Should().Be(1000);
        result.Dado!.FaltaReceber.Should().Be(6000);
        result.Dado!.FaltaPagar.Should().Be(2000);
        repos.ChamadasSomarReceitas.Should().Be(1);
        repos.ChamadasSomarDespesas.Should().Be(1);
    }
}