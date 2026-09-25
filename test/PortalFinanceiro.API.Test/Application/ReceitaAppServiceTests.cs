using FluentAssertions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.API.Test;

public class ReceitaAppServiceTests
{
    private sealed class ReceitaRepositoryFake : IReceitaRepository
    {
        public List<Receita> ParcelasInseridas { get; } = new();

        public Task<Receita?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id)
            => Task.FromResult<ReceitaProjecao?>(new ReceitaProjecao
            {
                Id = id,
                IdRegra = ParcelasInseridas.FirstOrDefault()?.IdRegra,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            });
        public Task<IEnumerable<ReceitaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null) => throw new NotImplementedException();
        public Task<int> ContarPorCategoriaAsync(Guid idCategoria) => throw new NotImplementedException();
        public Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria) => throw new NotImplementedException();
        public Task<int> ContarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Receita>> ListarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarPorContratoAsync(Guid idContrato) => throw new NotImplementedException();
        public Task InserirAsync(Receita entity) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<Receita> entities)
        {
            ParcelasInseridas.AddRange(entities);
            return Task.CompletedTask;
        }
        public Task AtualizarAsync(Receita entity) => throw new NotImplementedException();
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualCategoriaItem>> ResumoAnualPorCategoriaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
    }

    private sealed class RegraReceitaRepositoryFake : IRegraReceitaRepository
    {
        public RegraReceita? RegraInserida { get; private set; }

        public Task<RegraReceita?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<RegraReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<RegraReceitaProjecao>> ListarPorUsuarioAsync(Guid idUsuario) => throw new NotImplementedException();
        public Task InserirAsync(RegraReceita entity)
        {
            RegraInserida = entity;
            return Task.CompletedTask;
        }
        public Task AtualizarAsync(RegraReceita entity) => throw new NotImplementedException();
    }

    private sealed class ReceitaServicoRepositoryFake : IReceitaServicoRepository
    {
        public List<ReceitaServico> ServicosInseridos { get; } = new();

        public Task<IEnumerable<ReceitaServicoProjecao>> ListarPorReceitaAsync(Guid receitaId) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<ReceitaServico> entities)
        {
            ServicosInseridos.AddRange(entities);
            return Task.CompletedTask;
        }
        public Task ExcluirPorReceitaAsync(Guid receitaId) => throw new NotImplementedException();
    }

    private static ReceitaRequest RecorrenteValido(DateTime? dataFim = null)
        => new()
        {
            Descricao = "Aluguel",
            Valor = 1000,
            Data = new DateTime(2026, 1, 5),
            IdConta = Guid.NewGuid(),
            IdCategoria = Guid.NewGuid(),
            Repete = true,
            Dia = 5,
            DiaUtil = false,
            DataFim = dataFim ?? new DateTime(2026, 3, 31),
            Servicos = new List<ReceitaServicoRequest>
            {
                new() { CategoriaServicoId = Guid.NewGuid() },
                new() { CategoriaServicoId = Guid.NewGuid() }
            }
        };

    [Fact]
    public async Task Recorrente_GeraUmaParcelaPorMes_EPersisteRegraParcelasEServicos()
    {
        var receitas = new ReceitaRepositoryFake();
        var regras = new RegraReceitaRepositoryFake();
        var servicos = new ReceitaServicoRepositoryFake();
        var service = new ReceitaAppService(receitas, regras, servicos);
        var usuario = Guid.NewGuid();

        var result = await service.AdicionarAsync(usuario, RecorrenteValido());

        result.EhSucesso.Should().BeTrue();
        regras.RegraInserida.Should().NotBeNull();
        regras.RegraInserida!.Ativo.Should().BeTrue();
        receitas.ParcelasInseridas.Should().HaveCount(3);
        receitas.ParcelasInseridas.Select(p => (p.Data.Month, p.Data.Year))
            .Should().BeEquivalentTo([(1, 2026), (2, 2026), (3, 2026)]);
        receitas.ParcelasInseridas.Should().OnlyContain(p =>
            p.IdRegra == regras.RegraInserida.Id
            && p.Status == StatusMensal.Pendente
            && p.Ativo
            && p.IdUsuario == usuario);
        receitas.ParcelasInseridas.Select(p => p.Data.Day).Should().OnlyContain(d => d == 5);
        servicos.ServicosInseridos.Should().HaveCount(6);
        result.Dado!.EhRecorrente.Should().BeTrue();
        result.Dado!.IdRegra.Should().Be(regras.RegraInserida.Id);
    }

    [Fact]
    public async Task Recorrente_ComRegraInvalida_RetornaErroENaoPersisteNada()
    {
        var receitas = new ReceitaRepositoryFake();
        var regras = new RegraReceitaRepositoryFake();
        var servicos = new ReceitaServicoRepositoryFake();
        var service = new ReceitaAppService(receitas, regras, servicos);

        var result = await service.AdicionarAsync(Guid.NewGuid(), RecorrenteValido(new DateTime(2025, 12, 31)));

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("PERIODO_INVALIDO");
        regras.RegraInserida.Should().BeNull();
        receitas.ParcelasInseridas.Should().BeEmpty();
        servicos.ServicosInseridos.Should().BeEmpty();
    }
}