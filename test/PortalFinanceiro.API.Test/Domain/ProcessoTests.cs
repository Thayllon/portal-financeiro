using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class ProcessoTests
{
    private readonly Guid _idUsuario = Guid.NewGuid();
    private readonly Guid _idParceria = Guid.NewGuid();
    private readonly Guid _idContrato = Guid.NewGuid();

    [Fact]
    public void Criar_ComParceria_RetornaSucesso()
    {
        var result = Processo.Criar(_idUsuario, "Regularizar casa", "Casa no centro", _idParceria, null);

        result.EhSucesso.Should().BeTrue();
        result.Dado.Should().NotBeNull();
        result.Dado!.IdParceria.Should().Be(_idParceria);
        result.Dado.IdContrato.Should().BeNull();
        result.Dado.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Criar_ComContrato_RetornaSucesso()
    {
        var result = Processo.Criar(_idUsuario, "Regularizar casa", null, null, _idContrato);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.IdContrato.Should().Be(_idContrato);
    }

    [Fact]
    public void Criar_SemVinculo_RetornaValidacao()
    {
        var result = Processo.Criar(_idUsuario, "Regularizar casa", null, null, null);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("VINCULO_OBRIGATORIO");
    }

    [Fact]
    public void Criar_ComDoisVinculos_RetornaValidacao()
    {
        var result = Processo.Criar(_idUsuario, "Regularizar casa", null, _idParceria, _idContrato);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("VINCULO_OBRIGATORIO");
    }

    [Fact]
    public void Criar_SemNome_RetornaValidacao()
    {
        var result = Processo.Criar(_idUsuario, "", null, _idParceria, null);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("NOME_OBRIGATORIO");
    }

    [Fact]
    public void Atualizar_MantemVinculo_AlteraNomeEDescricao()
    {
        var processo = Processo.Criar(_idUsuario, "Antigo", null, _idParceria, null).Dado!;

        var result = processo.Atualizar("Novo", "Descricao");

        result.EhSucesso.Should().BeTrue();
        processo.Nome.Should().Be("Novo");
        processo.Descricao.Should().Be("Descricao");
        processo.IdParceria.Should().Be(_idParceria);
    }
}

[Trait("Categoria", "Dominio")]
public class ProcessoEtapaTests
{
    private readonly Guid _idProcesso = Guid.NewGuid();

    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = ProcessoEtapa.Criar(_idProcesso, "Cliente pagou", null, 1, DateTime.Today);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.Concluida.Should().BeFalse();
        result.Dado.DataConclusao.Should().BeNull();
        result.Dado.Ordem.Should().Be(1);
    }

    [Fact]
    public void Criar_ComOrdemZero_RetornaValidacao()
    {
        var result = ProcessoEtapa.Criar(_idProcesso, "Cliente pagou", null, 0, null);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("ORDEM_INVALIDA");
    }

    [Fact]
    public void MarcarConcluida_PreenchDataConclusao_Estornar_Limpa()
    {
        var etapa = ProcessoEtapa.Criar(_idProcesso, "Cliente pagou", null, 1, null).Dado!;

        etapa.MarcarConcluida();
        etapa.Concluida.Should().BeTrue();
        etapa.DataConclusao.Should().NotBeNull();

        etapa.Estornar();
        etapa.Concluida.Should().BeFalse();
        etapa.DataConclusao.Should().BeNull();
    }
}
