using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class CategoriaHistoricoTests
{
    private readonly Guid _idCategoria = Guid.NewGuid();
    private readonly Guid _idUsuario = Guid.NewGuid();

    [Fact]
    public void Criar_ComDadosValidos_RetornaEntidade()
    {
        var historico = CategoriaHistorico.Criar(
            _idCategoria,
            ETipoCategoria.Despesa,
            _idUsuario,
            EAcaoCategoriaHistorico.Criado,
            "Nome Antigo",
            "Nome Novo");

        historico.Should().NotBeNull();
        historico.Id.Should().NotBeEmpty();
        historico.IdCategoria.Should().Be(_idCategoria);
        historico.TipoCategoria.Should().Be(ETipoCategoria.Despesa);
        historico.IdUsuario.Should().Be(_idUsuario);
        historico.Acao.Should().Be(EAcaoCategoriaHistorico.Criado);
        historico.NomeAntigo.Should().Be("Nome Antigo");
        historico.NomeNovo.Should().Be("Nome Novo");
    }

    [Fact]
    public void Criar_ComAcaoEditado_DefinePropriedades()
    {
        var historico = CategoriaHistorico.Criar(
            _idCategoria,
            ETipoCategoria.Receita,
            _idUsuario,
            EAcaoCategoriaHistorico.Editado,
            categoriaPaiIdAntiga: Guid.NewGuid(),
            categoriaPaiIdNova: Guid.NewGuid());

        historico.Acao.Should().Be(EAcaoCategoriaHistorico.Editado);
        historico.CategoriaPaiIdAntiga.Should().NotBeNull();
        historico.CategoriaPaiIdNova.Should().NotBeNull();
    }

    [Fact]
    public void Criar_ComAcaoExcluido_SemNomes_RetornaEntidade()
    {
        var historico = CategoriaHistorico.Criar(
            _idCategoria,
            ETipoCategoria.Despesa,
            _idUsuario,
            EAcaoCategoriaHistorico.Excluido);

        historico.Acao.Should().Be(EAcaoCategoriaHistorico.Excluido);
        historico.NomeAntigo.Should().BeNull();
        historico.NomeNovo.Should().BeNull();
    }
}
