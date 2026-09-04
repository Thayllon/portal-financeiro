using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class UsuarioTests
{
    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = Usuario.Criar("João Silva", "joao@email.com", "hash123");

        result.EhSucesso.Should().BeTrue();
        result.Dado!.Nome.Should().Be("João Silva");
        result.Dado.Email.Should().Be("joao@email.com");
        result.Dado.SenhaHash.Should().Be("hash123");
        result.Dado.IsAdmin.Should().BeFalse();
        result.Dado.Ativo.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "joao@email.com", "hash123", "NOME_OBRIGATORIO")]
    [InlineData("   ", "joao@email.com", "hash123", "NOME_OBRIGATORIO")]
    [InlineData("João Silva", "", "hash123", "EMAIL_OBRIGATORIO")]
    [InlineData("João Silva", "joao@email.com", "", "SENHA_OBRIGATORIA")]
    public void Criar_CampoObrigatorioFaltando_RetornaValidacao(string nome, string email, string senhaHash, string codigoErro)
    {
        var result = Usuario.Criar(nome, email, senhaHash);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be(codigoErro);
    }

    [Fact]
    public void Criar_Admin_DefineIsAdminTrue()
    {
        var result = Usuario.Criar("Admin", "admin@email.com", "hash123", true);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public void Criar_NaoAdmin_DefineIsAdminFalse()
    {
        var result = Usuario.Criar("João Silva", "joao@email.com", "hash123");

        result.Dado!.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public void Atualizar_AlteraDados()
    {
        var criar = Usuario.Criar("João Silva", "joao@email.com", "hash123");
        var usuario = criar.Dado!;
        var dataAntes = usuario.DataAlteracao;

        var result = usuario.Atualizar("João Souza", "joao.novo@email.com");

        result.EhSucesso.Should().BeTrue();
        usuario.Nome.Should().Be("João Souza");
        usuario.Email.Should().Be("joao.novo@email.com");
        usuario.DataAlteracao.Should().BeAfter(dataAntes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_NomeInvalido_RetornaValidacao(string nome)
    {
        var criar = Usuario.Criar("João Silva", "joao@email.com", "hash123");
        var usuario = criar.Dado!;

        var result = usuario.Atualizar(nome, "joao@email.com");

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("NOME_OBRIGATORIO");
    }

    [Fact]
    public void AtualizarCompleta_AlteraDados()
    {
        var criar = Usuario.Criar("João Silva", "joao@email.com", "hash123");
        var usuario = criar.Dado!;

        var result = usuario.Atualizar("João Souza", "joao.novo@email.com", "novaHash456", true, false);

        result.EhSucesso.Should().BeTrue();
        usuario.Nome.Should().Be("João Souza");
        usuario.Email.Should().Be("joao.novo@email.com");
        usuario.SenhaHash.Should().Be("novaHash456");
        usuario.IsAdmin.Should().BeTrue();
        usuario.Ativo.Should().BeFalse();
    }

    [Fact]
    public void AtualizarCompleta_SenhaNula_NaoAlteraSenha()
    {
        var criar = Usuario.Criar("João Silva", "joao@email.com", "hash123");
        var usuario = criar.Dado!;

        var result = usuario.Atualizar("João Souza", "joao.novo@email.com", null, true, true);

        result.EhSucesso.Should().BeTrue();
        usuario.SenhaHash.Should().Be("hash123");
    }

    [Fact]
    public void Desativar_AlteraAtivo()
    {
        var criar = Usuario.Criar("João Silva", "joao@email.com", "hash123");
        var usuario = criar.Dado!;

        usuario.Desativar();

        usuario.Ativo.Should().BeFalse();
    }
}
