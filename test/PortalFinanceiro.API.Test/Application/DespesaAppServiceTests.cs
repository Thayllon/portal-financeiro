using FluentAssertions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.API.Test;

public class DespesaAppServiceTests
{
    private sealed class DespesaRepositoryFake : IDespesaRepository
    {
        public List<Despesa> ParcelasInseridas { get; } = new();

        public Task<Despesa?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<DespesaProjecao?> ObterProjecaoPorIdAsync(Guid id)
            => Task.FromResult<DespesaProjecao?>(new DespesaProjecao
            {
                Id = id,
                IdRegra = ParcelasInseridas.FirstOrDefault()?.IdRegra,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            });
        public Task<IEnumerable<DespesaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null) => throw new NotImplementedException();
        public Task<int> ContarPorCategoriaAsync(Guid idCategoria) => throw new NotImplementedException();
        public Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria) => throw new NotImplementedException();
        public Task<int> ContarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Despesa>> ListarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Despesa>> ListarPorReceitaOrigemAsync(Guid idReceitaOrigem) => throw new NotImplementedException();
        public Task<IEnumerable<DespesaProjecao>> ListarPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task InserirAsync(Despesa entity) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<Despesa> entities)
        {
            ParcelasInseridas.AddRange(entities);
            return Task.CompletedTask;
        }
        public Task AtualizarAsync(Despesa entity) => throw new NotImplementedException();
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualCategoriaItem>> ResumoAnualPorCategoriaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
    }

    private sealed class RegraDespesaRepositoryFake : IRegraDespesaRepository
    {
        public RegraDespesa? RegraInserida { get; private set; }

        public Task<RegraDespesa?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<RegraDespesaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<RegraDespesaProjecao>> ListarPorUsuarioAsync(Guid idUsuario) => throw new NotImplementedException();
        public Task InserirAsync(RegraDespesa entity)
        {
            RegraInserida = entity;
            return Task.CompletedTask;
        }
        public Task AtualizarAsync(RegraDespesa entity) => throw new NotImplementedException();
    }

    private sealed class DespesaServicoRepositoryFake : IDespesaServicoRepository
    {
        public List<DespesaServico> ServicosInseridos { get; } = new();

        public Task<IEnumerable<DespesaServico>> ListarPorDespesaAsync(Guid despesaId) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<DespesaServico> entities)
        {
            ServicosInseridos.AddRange(entities);
            return Task.CompletedTask;
        }
        public Task ExcluirPorDespesaAsync(Guid despesaId) => throw new NotImplementedException();
    }

    private static DespesaRequest RecorrenteValido(DateTime? dataFim = null)
        => new()
        {
            Descricao = "Internet",
            Valor = 150,
            Data = new DateTime(2026, 1, 5),
            IdConta = Guid.NewGuid(),
            IdCategoria = Guid.NewGuid(),
            Repete = true,
            Dia = 5,
            DiaUtil = false,
            DataFim = dataFim ?? new DateTime(2026, 3, 31),
            Servicos = new List<DespesaServicoRequest>
            {
                new() { CategoriaServicoId = Guid.NewGuid() }
            }
        };

    [Fact]
    public async Task Recorrente_GeraUmaParcelaPorMes_EPersisteRegraParcelasEServicos()
    {
        var despesas = new DespesaRepositoryFake();
        var regras = new RegraDespesaRepositoryFake();
        var servicos = new DespesaServicoRepositoryFake();
        var service = new DespesaAppService(despesas, regras, servicos);
        var usuario = Guid.NewGuid();

        var result = await service.AdicionarAsync(usuario, RecorrenteValido());

        result.EhSucesso.Should().BeTrue();
        regras.RegraInserida.Should().NotBeNull();
        regras.RegraInserida!.Ativo.Should().BeTrue();
        despesas.ParcelasInseridas.Should().HaveCount(3);
        despesas.ParcelasInseridas.Select(p => (p.Data.Month, p.Data.Year))
            .Should().BeEquivalentTo([(1, 2026), (2, 2026), (3, 2026)]);
        despesas.ParcelasInseridas.Should().OnlyContain(p =>
            p.IdRegra == regras.RegraInserida.Id
            && p.Status == StatusMensal.Pendente
            && p.Ativo
            && p.IdUsuario == usuario);
        servicos.ServicosInseridos.Should().HaveCount(3);
        result.Dado!.EhRecorrente.Should().BeTrue();
        result.Dado!.IdRegra.Should().Be(regras.RegraInserida.Id);
    }

    [Fact]
    public async Task Recorrente_ComRegraInvalida_RetornaErroENaoPersisteNada()
    {
        var despesas = new DespesaRepositoryFake();
        var regras = new RegraDespesaRepositoryFake();
        var servicos = new DespesaServicoRepositoryFake();
        var service = new DespesaAppService(despesas, regras, servicos);

        var result = await service.AdicionarAsync(Guid.NewGuid(), RecorrenteValido(new DateTime(2025, 12, 31)));

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("PERIODO_INVALIDO");
        regras.RegraInserida.Should().BeNull();
        despesas.ParcelasInseridas.Should().BeEmpty();
        servicos.ServicosInseridos.Should().BeEmpty();
    }
}