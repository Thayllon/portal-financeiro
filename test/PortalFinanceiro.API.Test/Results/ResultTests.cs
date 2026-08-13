using FluentAssertions;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Resultado")]
public class ResultTests
{
    [Fact]
    public void Sucesso_ComDados_PropertiesCorrect()
    {
        var result = Result<string>.Sucesso("teste");

        result.EhSucesso.Should().BeTrue();
        result.Dado.Should().Be("teste");
        result.Erro.Should().BeNull();
    }

    [Fact]
    public void Falha_ComErro_PropertiesCorrect()
    {
        var erro = Erro.Validacao("ERRO_TESTE", "Mensagem de erro");
        var result = Result<string>.Falha(erro);

        result.EhSucesso.Should().BeFalse();
        result.Dado.Should().BeNull();
        result.Erro.Should().Be(erro);
    }

    [Fact]
    public void ImplicitConversion_FromData_ReturnsSucesso()
    {
        Result<string> result = "teste";

        result.EhSucesso.Should().BeTrue();
        result.Dado.Should().Be("teste");
    }

    [Fact]
    public void ImplicitConversion_FromErro_ReturnsFalha()
    {
        Erro erro = Erro.Validacao("ERRO", "msg");
        Result<string> result = erro;

        result.EhSucesso.Should().BeFalse();
        result.Erro.Should().Be(erro);
    }
}

[Trait("Categoria", "Resultado")]
public class ErroTests
{
    [Fact]
    public void Validacao_CriaErroComTipoValidacao()
    {
        var erro = Erro.Validacao("CODIGO", "Mensagem");
        erro.Codigo.Should().Be("CODIGO");
        erro.Mensagem.Should().Be("Mensagem");
        erro.Tipo.Should().Be(ETipoErro.Validacao);
    }

    [Fact]
    public void Negocio_CriaErroComTipoNegocio()
    {
        var erro = Erro.Negocio("CODIGO", "Mensagem");
        erro.Tipo.Should().Be(ETipoErro.Negocio);
    }

    [Fact]
    public void NaoEncontrado_CriaErroComCodigoPadrao()
    {
        var erro = Erro.NaoEncontrado("Usuario");
        erro.Codigo.Should().Be("NAO_ENCONTRADO");
        erro.Mensagem.Should().Contain("Usuario");
        erro.Tipo.Should().Be(ETipoErro.NaoEncontrado);
    }

    [Fact]
    public void Conflito_CriaErroComTipoConflito() =>
        Erro.Conflito("CODIGO", "Mensagem").Tipo.Should().Be(ETipoErro.Conflito);

    [Fact]
    public void Permissao_CriaErroComTipoPermissao() =>
        Erro.Permissao("CODIGO", "Mensagem").Tipo.Should().Be(ETipoErro.Permissao);

    [Fact]
    public void Externo_CriaErroComTipoExterno() =>
        Erro.Externo("CODIGO", "Mensagem").Tipo.Should().Be(ETipoErro.Externo);

    [Fact]
    public void Infraestrutura_CriaErroComCodigoPadrao()
    {
        var erro = Erro.Infraestrutura("Mensagem");
        erro.Codigo.Should().Be("ERRO_INFRA");
        erro.Tipo.Should().Be(ETipoErro.Infraestrutura);
    }
}

[Trait("Categoria", "Resultado")]
public class UnitTests
{
    [Fact]
    public void Value_RetornaInstanciaUnica() =>
        Unit.Value.Should().BeSameAs(Unit.Value);
}

[Trait("Categoria", "Resultado")]
public class ResultadoTests
{
    [Fact]
    public void Sucesso_RetornaResultUnitComSucesso()
    {
        var result = Resultado.Sucesso();
        result.EhSucesso.Should().BeTrue();
        result.Dado.Should().Be(Unit.Value);
    }
}
