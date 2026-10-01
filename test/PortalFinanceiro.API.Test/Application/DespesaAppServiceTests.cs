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
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualRealizadoPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoRealizadoContaItem>> ResumoAnualRealizadoPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<decimal> ResumoMensalRealizadoAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoRealizadoContaItem>> ResumoMensalRealizadoPorContaAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null) => throw new NotImplementedException();
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

    private sealed class ParceriaRepositoryFake : IParceriaRepository
    {
        private readonly Parceria? _parceria;

        public ParceriaRepositoryFake(Parceria? parceria) => _parceria = parceria;

        public Task<Parceria?> ObterPorIdAsync(Guid id) => Task.FromResult(_parceria);
        public Task<ParceriaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ParceriaProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null) => throw new NotImplementedException();
        public Task<IEnumerable<ParceriaProjecao>> ListarComTotaisAsync(Guid idUsuario, bool? ativo, int statusRealizado) => throw new NotImplementedException();
        public Task InserirAsync(Parceria entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Parceria entity) => throw new NotImplementedException();
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<decimal> SomarReceitasPorStatusAsync(Guid idParceria, int status) => throw new NotImplementedException();
        public Task<decimal> SomarDespesasPorStatusAsync(Guid idParceria, int status) => throw new NotImplementedException();
        public Task<ResumoParceriaAnual> ResumoAnualAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<ResumoParceriaAnual> ResumoMensalAsync(Guid idUsuario, int ano, int mes, Guid? idConta = null) => throw new NotImplementedException();
    }

    private sealed class DespesaRepositoryFakeComObter : IDespesaRepository
    {
        private readonly Despesa _despesa;

        public DespesaRepositoryFakeComObter(Despesa despesa) => _despesa = despesa;

        public Task<Despesa?> ObterPorIdAsync(Guid id) => Task.FromResult<Despesa?>(_despesa);
        public Task<DespesaProjecao?> ObterProjecaoPorIdAsync(Guid id) => Task.FromResult<DespesaProjecao?>(new DespesaProjecao
        {
            Id = _despesa.Id,
            IdUsuario = _despesa.IdUsuario,
            Descricao = _despesa.Descricao,
            Valor = _despesa.Valor,
            Data = _despesa.Data,
            IdConta = _despesa.IdConta,
            IdCategoria = _despesa.IdCategoria,
            IdSubcategoria = _despesa.IdSubcategoria,
            IdParceria = _despesa.IdParceria,
            Status = _despesa.Status,
            DataRealizacao = _despesa.DataRealizacao,
            IdRegra = _despesa.IdRegra,
            Ativo = _despesa.Ativo,
            DataCadastro = _despesa.DataCadastro
        });
        public Task<IEnumerable<DespesaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null) => throw new NotImplementedException();
        public Task<int> ContarPorCategoriaAsync(Guid idCategoria) => throw new NotImplementedException();
        public Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria) => throw new NotImplementedException();
        public Task<int> ContarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Despesa>> ListarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Despesa>> ListarPorReceitaOrigemAsync(Guid idReceitaOrigem) => throw new NotImplementedException();
        public Task<IEnumerable<DespesaProjecao>> ListarPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task InserirAsync(Despesa entity) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<Despesa> entities) => throw new NotImplementedException();
        public Task AtualizarAsync(Despesa entity) => Task.CompletedTask;
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualCategoriaItem>> ResumoAnualPorCategoriaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualRealizadoPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoRealizadoContaItem>> ResumoAnualRealizadoPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<decimal> ResumoMensalRealizadoAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoRealizadoContaItem>> ResumoMensalRealizadoPorContaAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null) => throw new NotImplementedException();
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

    [Fact]
    public async Task Atualizar_MantendoParceriaEncerrada_PermiteSalvar()
    {
        var usuario = Guid.NewGuid();
        var parceriaId = Guid.NewGuid();
        var despesa = Despesa.Criar(usuario, "Saída", 300, new DateTime(2026, 3, 8), Guid.NewGuid(), Guid.NewGuid(), null, idParceria: parceriaId).Dado!;
        var fakes = new DespesaRepositoryFakeComObter(despesa);
        var parceriaEncerrada = Parceria.Criar(usuario, "Parceria X", Guid.NewGuid(), Guid.NewGuid(), 1000, 50).Dado!;
        parceriaEncerrada.Desativar();
        var service = new DespesaAppService(fakes, new RegraDespesaRepositoryFake(), new DespesaServicoRepositoryFake(),
            parceriaRepository: new ParceriaRepositoryFake(parceriaEncerrada));

        var request = new DespesaRequest
        {
            Descricao = "Saída editada",
            Valor = 350,
            Data = new DateTime(2026, 3, 8),
            IdConta = despesa.IdConta,
            IdCategoria = despesa.IdCategoria,
            IdParceria = parceriaId
        };

        var result = await service.AtualizarAsync(despesa.Id, usuario, request);

        result.EhSucesso.Should().BeTrue();
    }

    [Fact]
    public async Task Atualizar_TrocandoParaParceriaEncerrada_RetornaErro()
    {
        var usuario = Guid.NewGuid();
        var despesa = Despesa.Criar(usuario, "Saída", 300, new DateTime(2026, 3, 8), Guid.NewGuid(), Guid.NewGuid(), null).Dado!;
        var fakes = new DespesaRepositoryFakeComObter(despesa);
        var parceriaEncerrada = Parceria.Criar(usuario, "Parceria X", Guid.NewGuid(), Guid.NewGuid(), 1000, 50).Dado!;
        parceriaEncerrada.Desativar();
        var service = new DespesaAppService(fakes, new RegraDespesaRepositoryFake(), new DespesaServicoRepositoryFake(),
            parceriaRepository: new ParceriaRepositoryFake(parceriaEncerrada));

        var request = new DespesaRequest
        {
            Descricao = "Saída",
            Valor = 300,
            Data = new DateTime(2026, 3, 8),
            IdConta = despesa.IdConta,
            IdCategoria = despesa.IdCategoria,
            IdParceria = parceriaEncerrada.Id
        };

        var result = await service.AtualizarAsync(despesa.Id, usuario, request);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("PARCERIA_INVALIDA");
    }
}