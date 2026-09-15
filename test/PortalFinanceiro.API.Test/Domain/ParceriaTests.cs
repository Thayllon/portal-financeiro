using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class ParceriaTests
{
    private readonly Guid _idUsuario = Guid.NewGuid();
    private readonly Guid _idParceiro = Guid.NewGuid();
    private readonly Guid _idCliente = Guid.NewGuid();

    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = Parceria.Criar(_idUsuario, "Site NKG", _idParceiro, _idCliente, 4000m, 50m);

        result.EhSucesso.Should().BeTrue();
        result.Dado.Should().NotBeNull();
        result.Dado!.Nome.Should().Be("Site NKG");
        result.Dado.PercentualParceiro.Should().Be(50m);
        result.Dado.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Criar_SemNome_RetornaValidacao()
    {
        var result = Parceria.Criar(_idUsuario, "", _idParceiro, _idCliente, 4000m, 50m);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("NOME_OBRIGATORIO");
    }

    [Fact]
    public void Criar_ComPercentualMaiorQue100_RetornaValidacao()
    {
        var result = Parceria.Criar(_idUsuario, "Site NKG", _idParceiro, _idCliente, 4000m, 101m);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("PERCENTUAL_INVALIDO");
    }

    [Fact]
    public void Criar_ComPercentualNegativo_RetornaValidacao()
    {
        var result = Parceria.Criar(_idUsuario, "Site NKG", _idParceiro, _idCliente, 4000m, -1m);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("PERCENTUAL_INVALIDO");
    }

    [Fact]
    public void Atualizar_ComDadosValidos_AtualizaCampos()
    {
        var parceria = Parceria.Criar(_idUsuario, "Site NKG", _idParceiro, _idCliente, 4000m, 50m).Dado!;

        var result = parceria.Atualizar("Site NKG v2", _idParceiro, _idCliente, 5000m, 40m);

        result.EhSucesso.Should().BeTrue();
        parceria.Nome.Should().Be("Site NKG v2");
        parceria.Valor.Should().Be(5000m);
        parceria.PercentualParceiro.Should().Be(40m);
    }
}
