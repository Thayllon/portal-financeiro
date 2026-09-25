using FluentAssertions;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.API.Test;

public class ContratoAppServiceTests
{
    private sealed class ContratoRepositoryFake : IContratoRepository
    {
        public List<ContratoProjecao> Itens { get; } = new();
        public ContratoProjecao? PorId { get; set; }
        public int ChamadasSomarReceitas { get; private set; }
        public decimal SomaReceitas { get; set; }

        public Task<Contrato?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ContratoProjecao?> ObterProjecaoPorIdAsync(Guid id) => Task.FromResult(PorId);
        public Task<IEnumerable<ContratoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null, bool? ehRecorrente = null) => throw new NotImplementedException();
        public Task<IEnumerable<ContratoProjecao>> ListarComTotaisAsync(Guid idUsuario, bool? ativo, bool? ehRecorrente, int statusRealizado)
            => Task.FromResult<IEnumerable<ContratoProjecao>>(Itens.ToList());
        public Task InserirAsync(Contrato entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Contrato entity) => throw new NotImplementedException();
        public Task<decimal> SomarReceitasPorStatusAsync(Guid idContrato, int status)
        {
            ChamadasSomarReceitas++;
            return Task.FromResult(SomaReceitas);
        }
    }

    private sealed class PessoaRepositoryFake : IPessoaRepository
    {
        public Task<Pessoa?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<Pessoa>> ListarPorUsuarioAsync(Guid idUsuario) => throw new NotImplementedException();
        public Task InserirAsync(Pessoa entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Pessoa entity) => throw new NotImplementedException();
    }

    private static ContratoProjecao NovaProjecao(Guid usuario, decimal totalRecebido = 0)
        => new()
        {
            Id = Guid.NewGuid(),
            IdUsuario = usuario,
            Nome = "Contrato",
            IdCliente = Guid.NewGuid(),
            Cliente = "Cliente",
            Valor = 12000,
            Ativo = true,
            EhRecorrente = false,
            DataCadastro = DateTime.UtcNow,
            TotalRecebido = totalRecebido
        };

    [Fact]
    public async Task Listar_UsaTotalDaProjecao_SemQueryPorItem()
    {
        var repos = new ContratoRepositoryFake();
        var usuario = Guid.NewGuid();
        repos.Itens.Add(NovaProjecao(usuario, totalRecebido: 3000));
        repos.Itens.Add(NovaProjecao(usuario, totalRecebido: 5000));
        var service = new ContratoAppService(repos, new PessoaRepositoryFake());

        var result = await service.ListarAsync(usuario);

        result.EhSucesso.Should().BeTrue();
        var lista = result.Dado!.ToList();
        lista.Should().HaveCount(2);
        lista[0].TotalRecebido.Should().Be(3000);
        lista[0].FaltaReceber.Should().Be(9000);
        lista[1].TotalRecebido.Should().Be(5000);
        lista[1].FaltaReceber.Should().Be(7000);
        repos.ChamadasSomarReceitas.Should().Be(0);
    }

    [Fact]
    public async Task ObterPorId_BuscaTotalIndividualmente()
    {
        var repos = new ContratoRepositoryFake();
        var usuario = Guid.NewGuid();
        repos.PorId = NovaProjecao(usuario);
        repos.SomaReceitas = 3000;
        var service = new ContratoAppService(repos, new PessoaRepositoryFake());

        var result = await service.ObterPorIdAsync(repos.PorId.Id, usuario);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.TotalRecebido.Should().Be(3000);
        result.Dado!.FaltaReceber.Should().Be(9000);
        repos.ChamadasSomarReceitas.Should().Be(1);
    }
}