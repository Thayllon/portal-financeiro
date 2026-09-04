using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class ReceitaTests
{
    private readonly Guid _idUsuario = Guid.NewGuid();
    private readonly Guid _idConta = Guid.NewGuid();
    private readonly Guid _idCategoria = Guid.NewGuid();

    public record CriarParams(Guid IdUsuario, string Descricao, decimal Valor, Guid IdConta, Guid IdCategoria, Guid? IdSubcategoria, Guid? IdRegra);

    public static readonly TheoryData<CriarParams, string> ObterDadosCriarValidacao = new()
    {
        { new(Guid.Empty, "Salário", 5000m, Guid.NewGuid(), Guid.NewGuid(), null, null), "USUARIO_OBRIGATORIO" },
        { new(Guid.NewGuid(), "", 5000m, Guid.NewGuid(), Guid.NewGuid(), null, null), "DESCRICAO_OBRIGATORIA" },
        { new(Guid.NewGuid(), "Salário", 0, Guid.NewGuid(), Guid.NewGuid(), null, null), "VALOR_INVALIDO" },
        { new(Guid.NewGuid(), "Salário", -1, Guid.NewGuid(), Guid.NewGuid(), null, null), "VALOR_INVALIDO" },
        { new(Guid.NewGuid(), "Salário", 5000m, Guid.Empty, Guid.NewGuid(), null, null), "CONTA_OBRIGATORIA" },
        { new(Guid.NewGuid(), "Salário", 5000m, Guid.NewGuid(), Guid.Empty, null, null), "CATEGORIA_OBRIGATORIA" },
    };

    [Theory]
    [MemberData(nameof(ObterDadosCriarValidacao))]
    public void Criar_DadosInvalidos_RetornaValidacao(CriarParams p, string codigoErro)
    {
        var result = Receita.Criar(p.IdUsuario, p.Descricao, p.Valor, DateTime.Now, p.IdConta, p.IdCategoria, p.IdSubcategoria, p.IdRegra);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be(codigoErro);
    }

    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.IdUsuario.Should().Be(_idUsuario);
        result.Dado.Descricao.Should().Be("Salário");
        result.Dado.Valor.Should().Be(5000m);
        result.Dado.IdConta.Should().Be(_idConta);
        result.Dado.IdCategoria.Should().Be(_idCategoria);
        result.Dado.IdSubcategoria.Should().BeNull();
        result.Dado.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Criar_ComSubcategoria_DefineIdSubcategoria()
    {
        var idSub = Guid.NewGuid();
        var result = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, idSub);

        result.Dado!.IdSubcategoria.Should().Be(idSub);
    }

    [Fact]
    public void Criar_ComDadosValidos_StatusInicialPendente()
    {
        var result = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);

        result.Dado!.Status.Should().Be(StatusMensal.Pendente);
        result.Dado.DataRealizacao.Should().BeNull();
    }

    [Fact]
    public void Criar_ComIdRegra_EhRecorrenteTrue()
    {
        var idRegra = Guid.NewGuid();
        var result = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null, idRegra);

        result.Dado!.EhRecorrente.Should().BeTrue();
        result.Dado.IdRegra.Should().Be(idRegra);
    }

    [Fact]
    public void Criar_SemIdRegra_EhRecorrenteFalse()
    {
        var result = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);

        result.Dado!.EhRecorrente.Should().BeFalse();
        result.Dado.IdRegra.Should().BeNull();
    }

    [Fact]
    public void Atualizar_AlteraDados()
    {
        var criar = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);
        var receita = criar.Dado!;
        var novaConta = Guid.NewGuid();
        var novaCategoria = Guid.NewGuid();
        var dataAntes = receita.DataAlteracao;

        var result = receita.Atualizar("Freela", 3000m, DateTime.Now, novaConta, novaCategoria, null);

        result.EhSucesso.Should().BeTrue();
        receita.Descricao.Should().Be("Freela");
        receita.Valor.Should().Be(3000m);
        receita.IdConta.Should().Be(novaConta);
        receita.IdCategoria.Should().Be(novaCategoria);
        receita.DataAlteracao.Should().BeAfter(dataAntes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DescricaoInvalida_RetornaValidacao(string descricao)
    {
        var criar = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);
        var receita = criar.Dado!;

        var result = receita.Atualizar(descricao, 5000m, DateTime.Now, _idConta, _idCategoria, null);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("DESCRICAO_OBRIGATORIA");
    }

    [Fact]
    public void Receber_AlteraStatus()
    {
        var criar = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);
        var receita = criar.Dado!;
        var dataRecebimento = DateTime.UtcNow;

        var result = receita.Receber(dataRecebimento);

        result.EhSucesso.Should().BeTrue();
        receita.Status.Should().Be(StatusMensal.Realizado);
        receita.DataRealizacao.Should().Be(dataRecebimento);
        receita.DataAlteracao.Should().BeOnOrAfter(criar.Dado!.DataAlteracao);
    }

    [Fact]
    public void Receber_ReceitaJaRecebida_RetornaErro()
    {
        var criar = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);
        var receita = criar.Dado!;
        receita.Receber(DateTime.UtcNow);

        var result = receita.Receber(DateTime.UtcNow);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("RECEITA_JA_RECEBIDA");
    }

    [Fact]
    public void Estornar_AlteraStatus()
    {
        var criar = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);
        var receita = criar.Dado!;
        receita.Receber(DateTime.UtcNow);

        var result = receita.Estornar();

        result.EhSucesso.Should().BeTrue();
        receita.Status.Should().Be(StatusMensal.Pendente);
        receita.DataRealizacao.Should().BeNull();
    }

    [Fact]
    public void Estornar_ReceitaNaoRecebida_RetornaErro()
    {
        var criar = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);
        var receita = criar.Dado!;

        var result = receita.Estornar();

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("RECEITA_NAO_RECEBIDA");
    }

    [Fact]
    public void Desativar_AlteraAtivo()
    {
        var criar = Receita.Criar(_idUsuario, "Salário", 5000m, DateTime.Now, _idConta, _idCategoria, null);
        var receita = criar.Dado!;

        receita.Desativar();

        receita.Ativo.Should().BeFalse();
    }
}
