using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class CategoriaDespesaTests
{
    private readonly Guid _idUsuario = Guid.NewGuid();

    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = CategoriaDespesa.Criar(_idUsuario, "Alimentação");

        result.EhSucesso.Should().BeTrue();
        result.Dado.Should().NotBeNull();
        result.Dado!.IdUsuario.Should().Be(_idUsuario);
        result.Dado.Nome.Should().Be("Alimentação");
        result.Dado.CategoriaPaiId.Should().BeNull();
        result.Dado.Ativo.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "NOME_OBRIGATORIO")]
    [InlineData("   ", "NOME_OBRIGATORIO")]
    public void Criar_NomeInvalido_RetornaValidacao(string nome, string codigoErro)
    {
        var result = CategoriaDespesa.Criar(_idUsuario, nome);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be(codigoErro);
    }

    [Fact]
    public void Criar_SemUsuario_RetornaValidacao()
    {
        var result = CategoriaDespesa.Criar(Guid.Empty, "Alimentação");
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("USUARIO_OBRIGATORIO");
    }

    [Fact]
    public void Criar_ComCategoriaPai_DefineCategoriaPaiId()
    {
        var categoriaPaiId = Guid.NewGuid();
        var result = CategoriaDespesa.Criar(_idUsuario, "Alimentação", categoriaPaiId);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.CategoriaPaiId.Should().Be(categoriaPaiId);
    }

    [Fact]
    public void Atualizar_AlteraDados()
    {
        var criar = CategoriaDespesa.Criar(_idUsuario, "Alimentação");
        var categoria = criar.Dado!;
        var novoId = Guid.NewGuid();
        var dataAntes = categoria.DataAlteracao;

        var result = categoria.Atualizar("Refeição", novoId);

        result.EhSucesso.Should().BeTrue();
        categoria.Nome.Should().Be("Refeição");
        categoria.CategoriaPaiId.Should().Be(novoId);
        categoria.DataAlteracao.Should().BeAfter(dataAntes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_NomeInvalido_RetornaValidacao(string nome)
    {
        var criar = CategoriaDespesa.Criar(_idUsuario, "Alimentação");
        var categoria = criar.Dado!;

        var result = categoria.Atualizar(nome, null);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("NOME_OBRIGATORIO");
    }

    [Fact]
    public void Desativar_AlteraAtivo()
    {
        var criar = CategoriaDespesa.Criar(_idUsuario, "Alimentação");
        var categoria = criar.Dado!;
        var dataAntes = categoria.DataAlteracao;

        categoria.Desativar();

        categoria.Ativo.Should().BeFalse();
        categoria.DataAlteracao.Should().BeAfter(dataAntes);
    }
}
