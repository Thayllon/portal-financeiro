using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class PessoaTests
{
    private readonly Guid _idUsuario = Guid.NewGuid();

    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = Pessoa.Criar(_idUsuario, "João da Silva", "(11) 99999-9999", TipoPessoa.Cliente);

        result.EhSucesso.Should().BeTrue();
        result.Dado.Should().NotBeNull();
        result.Dado!.IdUsuario.Should().Be(_idUsuario);
        result.Dado.Nome.Should().Be("João da Silva");
        result.Dado.Telefone.Should().Be("(11) 99999-9999");
        result.Dado.Tipo.Should().Be(TipoPessoa.Cliente);
        result.Dado.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Criar_SemUsuario_RetornaValidacao()
    {
        var result = Pessoa.Criar(Guid.Empty, "João da Silva", null, TipoPessoa.Cliente);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("USUARIO_OBRIGATORIO");
    }

    [Fact]
    public void Criar_SemNome_RetornaValidacao()
    {
        var result = Pessoa.Criar(_idUsuario, "", null, TipoPessoa.Cliente);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("NOME_OBRIGATORIO");
    }

    [Fact]
    public void Criar_ComTipoCliente_DefineCliente()
    {
        var result = Pessoa.Criar(_idUsuario, "João da Silva", null, TipoPessoa.Cliente);
        result.Dado!.Tipo.Should().Be(TipoPessoa.Cliente);
    }

    [Fact]
    public void Criar_ComTipoParceiro_DefineParceiro()
    {
        var result = Pessoa.Criar(_idUsuario, "Mercado Bom", null, TipoPessoa.Parceiro);
        result.Dado!.Tipo.Should().Be(TipoPessoa.Parceiro);
    }

    [Fact]
    public void Criar_SemTelefone_RetornaTelefoneNulo()
    {
        var result = Pessoa.Criar(_idUsuario, "João da Silva", null, TipoPessoa.Cliente);
        result.Dado!.Telefone.Should().BeNull();
    }

    [Fact]
    public void Atualizar_AlteraDados()
    {
        var criar = Pessoa.Criar(_idUsuario, "João da Silva", "(11) 99999-9999", TipoPessoa.Cliente);
        var pessoa = criar.Dado!;
        var dataAntes = pessoa.DataAlteracao;

        var result = pessoa.Atualizar("João Souza", "(21) 98888-8888", TipoPessoa.Parceiro);

        result.EhSucesso.Should().BeTrue();
        pessoa.Nome.Should().Be("João Souza");
        pessoa.Telefone.Should().Be("(21) 98888-8888");
        pessoa.Tipo.Should().Be(TipoPessoa.Parceiro);
        pessoa.DataAlteracao.Should().BeAfter(dataAntes);
    }

    [Fact]
    public void Atualizar_SemNome_RetornaValidacao()
    {
        var criar = Pessoa.Criar(_idUsuario, "João da Silva", null, TipoPessoa.Cliente);
        var pessoa = criar.Dado!;

        var result = pessoa.Atualizar("", null, TipoPessoa.Cliente);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("NOME_OBRIGATORIO");
    }

    [Fact]
    public void Desativar_AlteraAtivo()
    {
        var criar = Pessoa.Criar(_idUsuario, "João da Silva", null, TipoPessoa.Cliente);
        var pessoa = criar.Dado!;

        pessoa.Desativar();

        pessoa.Ativo.Should().BeFalse();
    }
}