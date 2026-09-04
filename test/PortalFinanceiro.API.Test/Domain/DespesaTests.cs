using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class DespesaTests
{
    private readonly Guid _idUsuario = Guid.NewGuid();
    private readonly Guid _idConta = Guid.NewGuid();
    private readonly Guid _idCategoria = Guid.NewGuid();

    public record CriarParams(Guid IdUsuario, string Descricao, decimal Valor, Guid IdConta, Guid IdCategoria, Guid? IdSubcategoria, Guid? IdRegra);

    public static readonly TheoryData<CriarParams, string> ObterDadosCriarValidacao = new()
    {
        { new(Guid.Empty, "Aluguel", 1500m, Guid.NewGuid(), Guid.NewGuid(), null, null), "USUARIO_OBRIGATORIO" },
        { new(Guid.NewGuid(), "", 1500m, Guid.NewGuid(), Guid.NewGuid(), null, null), "DESCRICAO_OBRIGATORIA" },
        { new(Guid.NewGuid(), "Aluguel", 0, Guid.NewGuid(), Guid.NewGuid(), null, null), "VALOR_INVALIDO" },
        { new(Guid.NewGuid(), "Aluguel", -1, Guid.NewGuid(), Guid.NewGuid(), null, null), "VALOR_INVALIDO" },
        { new(Guid.NewGuid(), "Aluguel", 1500m, Guid.Empty, Guid.NewGuid(), null, null), "CONTA_OBRIGATORIA" },
        { new(Guid.NewGuid(), "Aluguel", 1500m, Guid.NewGuid(), Guid.Empty, null, null), "CATEGORIA_OBRIGATORIA" },
    };

    [Theory]
    [MemberData(nameof(ObterDadosCriarValidacao))]
    public void Criar_DadosInvalidos_RetornaValidacao(CriarParams p, string codigoErro)
    {
        var result = Despesa.Criar(p.IdUsuario, p.Descricao, p.Valor, DateTime.Now, p.IdConta, p.IdCategoria, p.IdSubcategoria, p.IdRegra);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be(codigoErro);
    }

    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.IdUsuario.Should().Be(_idUsuario);
        result.Dado.Descricao.Should().Be("Aluguel");
        result.Dado.Valor.Should().Be(1500m);
        result.Dado.IdConta.Should().Be(_idConta);
        result.Dado.IdCategoria.Should().Be(_idCategoria);
        result.Dado.IdSubcategoria.Should().BeNull();
        result.Dado.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Criar_ComSubcategoria_DefineIdSubcategoria()
    {
        var idSub = Guid.NewGuid();
        var result = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, idSub);

        result.Dado!.IdSubcategoria.Should().Be(idSub);
    }

    [Fact]
    public void Criar_ComDadosValidos_StatusInicialPendente()
    {
        var result = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);

        result.Dado!.Status.Should().Be(StatusMensal.Pendente);
        result.Dado.DataRealizacao.Should().BeNull();
    }

    [Fact]
    public void Criar_ComIdRegra_EhRecorrenteTrue()
    {
        var idRegra = Guid.NewGuid();
        var result = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null, idRegra);

        result.Dado!.EhRecorrente.Should().BeTrue();
        result.Dado.IdRegra.Should().Be(idRegra);
    }

    [Fact]
    public void Criar_SemIdRegra_EhRecorrenteFalse()
    {
        var result = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);

        result.Dado!.EhRecorrente.Should().BeFalse();
        result.Dado.IdRegra.Should().BeNull();
    }

    [Fact]
    public void Atualizar_AlteraDados()
    {
        var criar = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);
        var despesa = criar.Dado!;
        var novaConta = Guid.NewGuid();
        var novaCategoria = Guid.NewGuid();
        var dataAntes = despesa.DataAlteracao;

        var result = despesa.Atualizar("Aluguel Novo", 1800m, DateTime.Now, novaConta, novaCategoria, null);

        result.EhSucesso.Should().BeTrue();
        despesa.Descricao.Should().Be("Aluguel Novo");
        despesa.Valor.Should().Be(1800m);
        despesa.IdConta.Should().Be(novaConta);
        despesa.IdCategoria.Should().Be(novaCategoria);
        despesa.DataAlteracao.Should().BeAfter(dataAntes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DescricaoInvalida_RetornaValidacao(string descricao)
    {
        var criar = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);
        var despesa = criar.Dado!;

        var result = despesa.Atualizar(descricao, 1500m, DateTime.Now, _idConta, _idCategoria, null);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("DESCRICAO_OBRIGATORIA");
    }

    [Fact]
    public void Pagar_AlteraStatus()
    {
        var criar = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);
        var despesa = criar.Dado!;
        var dataPagamento = DateTime.UtcNow;

        var result = despesa.Pagar(dataPagamento);

        result.EhSucesso.Should().BeTrue();
        despesa.Status.Should().Be(StatusMensal.Realizado);
        despesa.DataRealizacao.Should().Be(dataPagamento);
        despesa.DataAlteracao.Should().BeOnOrAfter(criar.Dado!.DataAlteracao);
    }

    [Fact]
    public void Pagar_DespesaJaPaga_RetornaErro()
    {
        var criar = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);
        var despesa = criar.Dado!;
        despesa.Pagar(DateTime.UtcNow);

        var result = despesa.Pagar(DateTime.UtcNow);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("DESPESA_JA_PAGA");
    }

    [Fact]
    public void Estornar_AlteraStatus()
    {
        var criar = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);
        var despesa = criar.Dado!;
        despesa.Pagar(DateTime.UtcNow);

        var result = despesa.Estornar();

        result.EhSucesso.Should().BeTrue();
        despesa.Status.Should().Be(StatusMensal.Pendente);
        despesa.DataRealizacao.Should().BeNull();
    }

    [Fact]
    public void Estornar_DespesaNaoPaga_RetornaErro()
    {
        var criar = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);
        var despesa = criar.Dado!;

        var result = despesa.Estornar();

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("DESPESA_NAO_PAGA");
    }

    [Fact]
    public void Desativar_AlteraAtivo()
    {
        var criar = Despesa.Criar(_idUsuario, "Aluguel", 1500m, DateTime.Now, _idConta, _idCategoria, null);
        var despesa = criar.Dado!;

        despesa.Desativar();

        despesa.Ativo.Should().BeFalse();
    }
}
