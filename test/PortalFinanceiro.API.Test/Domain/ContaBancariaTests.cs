using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class ContaBancariaTests
{
    private readonly Guid _idUsuario = Guid.NewGuid();

    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = ContaBancaria.Criar(_idUsuario, "Nubank PF", "Nubank", TipoConta.Pf);

        result.EhSucesso.Should().BeTrue();
        result.Dado.Should().NotBeNull();
        result.Dado!.IdUsuario.Should().Be(_idUsuario);
        result.Dado.Nome.Should().Be("Nubank PF");
        result.Dado.Banco.Should().Be("Nubank");
        result.Dado.Tipo.Should().Be(TipoConta.Pf);
        result.Dado.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Criar_SemUsuario_RetornaValidacao()
    {
        var result = ContaBancaria.Criar(Guid.Empty, "Nubank PF", "Nubank", TipoConta.Pf);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("USUARIO_OBRIGATORIO");
    }

    [Fact]
    public void Criar_SemNome_RetornaValidacao()
    {
        var result = ContaBancaria.Criar(_idUsuario, "", "Nubank", TipoConta.Pf);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("NOME_OBRIGATORIO");
    }

    [Fact]
    public void Criar_SemBanco_RetornaValidacao()
    {
        var result = ContaBancaria.Criar(_idUsuario, "Nubank PF", "", TipoConta.Pf);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("BANCO_OBRIGATORIO");
    }

    [Fact]
    public void Criar_ComTipoPf_DefineTipoPf()
    {
        var result = ContaBancaria.Criar(_idUsuario, "Nubank PF", "Nubank", TipoConta.Pf);
        result.Dado!.Tipo.Should().Be(TipoConta.Pf);
    }

    [Fact]
    public void Criar_ComTipoPj_DefineTipoPj()
    {
        var result = ContaBancaria.Criar(_idUsuario, "Itau PJ", "Itau", TipoConta.Pj);
        result.Dado!.Tipo.Should().Be(TipoConta.Pj);
    }

    [Fact]
    public void Atualizar_AlteraDados()
    {
        var criar = ContaBancaria.Criar(_idUsuario, "Nubank PF", "Nubank", TipoConta.Pf);
        var conta = criar.Dado!;
        var dataAntes = conta.DataAlteracao;

        var result = conta.Atualizar("Inter PF", "Inter", TipoConta.Pf);

        result.EhSucesso.Should().BeTrue();
        conta.Nome.Should().Be("Inter PF");
        conta.Banco.Should().Be("Inter");
        conta.Tipo.Should().Be(TipoConta.Pf);
        conta.DataAlteracao.Should().BeAfter(dataAntes);
    }

    [Theory]
    [InlineData("", "Nubank", "NOME_OBRIGATORIO")]
    [InlineData("Nubank PF", "", "BANCO_OBRIGATORIO")]
    public void Atualizar_CampoObrigatorioFaltando_RetornaValidacao(string nome, string banco, string codigoErro)
    {
        var criar = ContaBancaria.Criar(_idUsuario, "Nubank PF", "Nubank", TipoConta.Pf);
        var conta = criar.Dado!;

        var result = conta.Atualizar(nome, banco, TipoConta.Pf);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be(codigoErro);
    }

    [Fact]
    public void Desativar_AlteraAtivo()
    {
        var criar = ContaBancaria.Criar(_idUsuario, "Nubank PF", "Nubank", TipoConta.Pf);
        var conta = criar.Dado!;

        conta.Desativar();

        conta.Ativo.Should().BeFalse();
    }
}
