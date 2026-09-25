using FluentAssertions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

public class CategoriaReceitaAppServiceTests
{
    private sealed class CategoriaReceitaRepositoryFake : ICategoriaReceitaRepository
    {
        public List<CategoriaReceita> Itens { get; } = new();
        public int Atualizacoes { get; private set; }
        public int Exclusoes { get; private set; }

        public Task<CategoriaReceita?> ObterPorIdAsync(Guid id)
            => Task.FromResult(Itens.FirstOrDefault(c => c.Id == id));

        public Task<IEnumerable<CategoriaReceita>> ListarAsync()
            => Task.FromResult<IEnumerable<CategoriaReceita>>(Itens.ToList());

        public Task<IEnumerable<CategoriaReceita>> ListarPorPaiAsync(Guid? categoriaPaiId)
            => Task.FromResult<IEnumerable<CategoriaReceita>>(Itens.Where(c => c.CategoriaPaiId == categoriaPaiId).ToList());

        public Task InserirAsync(CategoriaReceita entity)
        {
            Itens.Add(entity);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(CategoriaReceita entity)
        {
            Atualizacoes++;
            return Task.CompletedTask;
        }

        public Task ExcluirAsync(Guid id)
        {
            Exclusoes++;
            return Task.CompletedTask;
        }
    }

    private sealed class CategoriaHistoricoRepositoryFake : ICategoriaHistoricoRepository
    {
        public List<CategoriaHistorico> Registros { get; } = new();

        public Task InserirAsync(CategoriaHistorico entity)
        {
            Registros.Add(entity);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<CategoriaHistorico>> ListarPorCategoriaAsync(Guid idCategoria, ETipoCategoria tipoCategoria)
            => Task.FromResult<IEnumerable<CategoriaHistorico>>(Registros.Where(h => h.IdCategoria == idCategoria).ToList());
    }

    private sealed class ReceitaRepositoryFake : IReceitaRepository
    {
        public Task<Receita?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null) => throw new NotImplementedException();
        public Task<int> ContarPorCategoriaAsync(Guid idCategoria) => Task.FromResult(0);
        public Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria) => Task.FromResult(0);
        public Task<int> ContarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<Receita>> ListarPorRegraAsync(Guid idRegra) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task<IEnumerable<ReceitaProjecao>> ListarPorContratoAsync(Guid idContrato) => throw new NotImplementedException();
        public Task InserirAsync(Receita entity) => throw new NotImplementedException();
        public Task InserirEmMassaAsync(IEnumerable<Receita> entities) => throw new NotImplementedException();
        public Task AtualizarAsync(Receita entity) => throw new NotImplementedException();
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<IEnumerable<ResumoAnualCategoriaItem>> ResumoAnualPorCategoriaAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
    }

    private static (CategoriaReceitaAppService service, CategoriaReceitaRepositoryFake repos, CategoriaHistoricoRepositoryFake historico) CriarService()
    {
        var repos = new CategoriaReceitaRepositoryFake();
        var historico = new CategoriaHistoricoRepositoryFake();
        var service = new CategoriaReceitaAppService(repos, new ReceitaRepositoryFake(), historico);
        return (service, repos, historico);
    }

    private static async Task<Guid> CriarCategoria(CategoriaReceitaAppService service, Guid dono, string nome = "Categoria")
    {
        var criado = await service.AdicionarAsync(dono, new CategoriaRequest { Nome = nome });
        criado.EhSucesso.Should().BeTrue();
        return criado.Dado!.Id;
    }

    [Fact]
    public async Task DonoEditaPropriaCategoria_Sucesso()
    {
        var (service, repos, historico) = CriarService();
        var dono = Guid.NewGuid();
        var categoria = await CriarCategoria(service, dono);

        var result = await service.AtualizarAsync(categoria, dono, false, new CategoriaRequest { Nome = "Renomeada" });

        result.EhSucesso.Should().BeTrue();
        repos.Atualizacoes.Should().Be(1);
        result.Dado!.Nome.Should().Be("Renomeada");
    }

    [Fact]
    public async Task NaoDonoNaoAdminTentaEditar_RetornaPermissao()
    {
        var (service, repos, historico) = CriarService();
        var dono = Guid.NewGuid();
        var categoria = await CriarCategoria(service, dono);

        var result = await service.AtualizarAsync(categoria, Guid.NewGuid(), false, new CategoriaRequest { Nome = "Invadida" });

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Tipo.Should().Be(ETipoErro.Permissao);
        repos.Atualizacoes.Should().Be(0);
        historico.Registros.Should().NotContain(r => r.Acao == EAcaoCategoriaHistorico.Editado);
    }

    [Fact]
    public async Task AdminEditaCategoriaDeOutro_Sucesso()
    {
        var (service, repos, historico) = CriarService();
        var dono = Guid.NewGuid();
        var categoria = await CriarCategoria(service, dono);

        var result = await service.AtualizarAsync(categoria, Guid.NewGuid(), true, new CategoriaRequest { Nome = "Renomeada" });

        result.EhSucesso.Should().BeTrue();
        repos.Atualizacoes.Should().Be(1);
    }

    [Fact]
    public async Task NaoDonoNaoAdminTentaExcluir_RetornaPermissao()
    {
        var (service, repos, historico) = CriarService();
        var dono = Guid.NewGuid();
        var categoria = await CriarCategoria(service, dono);

        var result = await service.ExcluirAsync(categoria, Guid.NewGuid(), false);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Tipo.Should().Be(ETipoErro.Permissao);
        repos.Exclusoes.Should().Be(0);
        historico.Registros.Should().NotContain(r => r.Acao == EAcaoCategoriaHistorico.Excluido);
    }

    [Fact]
    public async Task DonoExcluiPropriaCategoria_SucessoEGravaAuditoria()
    {
        var (service, repos, historico) = CriarService();
        var dono = Guid.NewGuid();
        var categoria = await CriarCategoria(service, dono);

        var result = await service.ExcluirAsync(categoria, dono, false);

        result.EhSucesso.Should().BeTrue();
        repos.Exclusoes.Should().Be(1);
        historico.Registros.Should().ContainSingle(r => r.Acao == EAcaoCategoriaHistorico.Excluido);
    }

    [Fact]
    public async Task Atualizar_GravaAuditoriaDeEdicao()
    {
        var (service, repos, historico) = CriarService();
        var dono = Guid.NewGuid();
        var categoria = await CriarCategoria(service, dono);

        await service.AtualizarAsync(categoria, dono, false, new CategoriaRequest { Nome = "Renomeada" });

        historico.Registros.Should().ContainSingle(r => r.Acao == EAcaoCategoriaHistorico.Editado && r.NomeNovo == "Renomeada");
    }

    [Fact]
    public async Task Listar_DefinePodeEditarConformeDonoEOuAdmin()
    {
        var (service, repos, historico) = CriarService();
        var dono = Guid.NewGuid();
        await CriarCategoria(service, dono);

        var comoDono = await service.ListarAsync(dono, false);
        comoDono.Dado!.Single().PodeEditar.Should().BeTrue();

        var comoTerceiro = await service.ListarAsync(Guid.NewGuid(), false);
        comoTerceiro.Dado!.Single().PodeEditar.Should().BeFalse();

        var comoAdmin = await service.ListarAsync(Guid.NewGuid(), true);
        comoAdmin.Dado!.Single().PodeEditar.Should().BeTrue();
    }

    [Fact]
    public async Task CategoriaRecemCriada_RetornaPodeEditarTrueParaODono()
    {
        var (service, repos, historico) = CriarService();
        var dono = Guid.NewGuid();

        var result = await service.AdicionarAsync(dono, new CategoriaRequest { Nome = "Nova" });

        result.EhSucesso.Should().BeTrue();
        result.Dado!.PodeEditar.Should().BeTrue();
    }
}