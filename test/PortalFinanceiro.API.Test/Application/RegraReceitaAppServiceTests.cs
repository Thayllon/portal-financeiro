using FluentAssertions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.API.Test;

public class RegraReceitaAppServiceTests
{
    private sealed class RegraReceitaRepositoryFake : IRegraReceitaRepository
    {
        public RegraReceita? Regra { get; set; }
        public int Atualizacoes { get; private set; }

        public Task<RegraReceita?> ObterPorIdAsync(Guid id)
            => Task.FromResult(Regra is not null && Regra.Id == id ? Regra : null);

        public Task<RegraReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id)
            => Task.FromResult<RegraReceitaProjecao?>(Regra is null ? null : new RegraReceitaProjecao
            {
                Id = Regra.Id,
                IdUsuario = Regra.IdUsuario,
                Descricao = Regra.Descricao,
                Valor = Regra.Valor,
                Dia = Regra.Dia,
                DiaUtil = Regra.DiaUtil,
                IdCategoria = Regra.IdCategoria,
                IdConta = Regra.IdConta,
                DataInicio = Regra.DataInicio,
                DataFim = Regra.DataFim,
                Ativo = Regra.Ativo
            });

        public Task<IEnumerable<RegraReceitaProjecao>> ListarPorUsuarioAsync(Guid idUsuario) => throw new NotImplementedException();
        public Task InserirAsync(RegraReceita entity) => throw new NotImplementedException();
        public Task AtualizarAsync(RegraReceita entity)
        {
            Atualizacoes++;
            return Task.CompletedTask;
        }
    }

    private sealed class ReceitaRepositoryFake : IReceitaRepository
    {
        public List<Receita> Parcelas { get; } = new();
        public List<Receita> ParcelasAtualizadas { get; } = new();

        public Task<Receita?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null) => throw new NotImplementedException();
        public Task<int> ContarPorCategoriaAsync(Guid idCategoria) => throw new NotImplementedException();
        public Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria) => throw new NotImplementedException();
        public Task<int> ContarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Receita>> ListarPorRegraAsync(Guid idRegra)
            => Task.FromResult<IEnumerable<Receita>>(Parcelas.Where(p => p.IdRegra == idRegra).ToList());
        public Task<IEnumerable<ReceitaProjecao>> ListarPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarPorContratoAsync(Guid idContrato) => throw new NotImplementedException();
        public Task InserirAsync(Receita entity) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<Receita> entities) => throw new NotImplementedException();
        public Task AtualizarAsync(Receita entity)
        {
            ParcelasAtualizadas.Add(entity);
            return Task.CompletedTask;
        }
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualCategoriaItem>> ResumoAnualPorCategoriaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
    }

    private static RegraReceita CriarRegra(Guid usuario, Guid conta, Guid categoria)
    {
        var criada = RegraReceita.Criar(usuario, "Regra", 100, 5, false, categoria, conta,
            new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        criada.EhSucesso.Should().BeTrue();
        return criada.Dado!;
    }

    private static Receita CriarParcela(Guid usuario, Guid conta, Guid categoria, Guid regraId, DateTime data)
    {
        var criada = Receita.Criar(usuario, "Antiga", 100, data, conta, categoria, null, regraId);
        criada.EhSucesso.Should().BeTrue();
        return criada.Dado!;
    }

    private static RegraReceitaRequest NovoValor(Guid conta, Guid categoria)
        => new()
        {
            Descricao = "Nova",
            Valor = 200,
            Dia = 10,
            DiaUtil = false,
            IdCategoria = categoria,
            IdConta = conta,
            DataInicio = new DateTime(2026, 1, 1),
            DataFim = new DateTime(2026, 12, 31)
        };

    [Fact]
    public async Task Atualizar_PropagaSomenteParaParcelasPendentesFuturas()
    {
        var usuario = Guid.NewGuid();
        var conta = Guid.NewGuid();
        var categoria = Guid.NewGuid();
        var regra = CriarRegra(usuario, conta, categoria);

        var passada = CriarParcela(usuario, conta, categoria, regra.Id, new DateTime(2020, 1, 5));
        var futuraPendente = CriarParcela(usuario, conta, categoria, regra.Id, DateTime.UtcNow.AddMonths(2).Date);
        var futuraRealizada = CriarParcela(usuario, conta, categoria, regra.Id, DateTime.UtcNow.AddMonths(1).Date);
        futuraRealizada.Receber(DateTime.UtcNow).EhSucesso.Should().BeTrue();

        var regras = new RegraReceitaRepositoryFake { Regra = regra };
        var receitas = new ReceitaRepositoryFake();
        receitas.Parcelas.AddRange(new[] { passada, futuraPendente, futuraRealizada });
        var service = new RegraReceitaAppService(regras, receitas);

        var result = await service.AtualizarAsync(regra.Id, usuario, NovoValor(conta, categoria));

        result.EhSucesso.Should().BeTrue();
        receitas.ParcelasAtualizadas.Should().ContainSingle(p => p.Id == futuraPendente.Id);
        futuraPendente.Descricao.Should().Be("Nova");
        futuraPendente.Valor.Should().Be(200);
        futuraPendente.Data.Day.Should().Be(10);
        passada.Descricao.Should().Be("Antiga");
        futuraRealizada.Descricao.Should().Be("Antiga");
    }

    [Fact]
    public async Task Atualizar_ComDadosInvalidos_RetornaErroESemEfeitoColateral()
    {
        var usuario = Guid.NewGuid();
        var conta = Guid.NewGuid();
        var categoria = Guid.NewGuid();
        var regra = CriarRegra(usuario, conta, categoria);
        var parcela = CriarParcela(usuario, conta, categoria, regra.Id, DateTime.UtcNow.AddMonths(1).Date);

        var regras = new RegraReceitaRepositoryFake { Regra = regra };
        var receitas = new ReceitaRepositoryFake();
        receitas.Parcelas.Add(parcela);
        var service = new RegraReceitaAppService(regras, receitas);

        var invalido = NovoValor(conta, categoria);
        invalido.Valor = 0;

        var result = await service.AtualizarAsync(regra.Id, usuario, invalido);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("VALOR_INVALIDO");
        regras.Atualizacoes.Should().Be(0);
        receitas.ParcelasAtualizadas.Should().BeEmpty();
        parcela.Descricao.Should().Be("Antiga");
    }
}