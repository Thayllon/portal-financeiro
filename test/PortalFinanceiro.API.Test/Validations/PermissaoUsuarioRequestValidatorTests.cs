using FluentAssertions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Validations;
using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Validacao")]
public class PermissaoUsuarioRequestValidatorTests
{
    [Theory]
    [InlineData(NivelPermissao.Nenhum)]
    [InlineData(NivelPermissao.Leitura)]
    [InlineData(NivelPermissao.Escrita)]
    public void Validar_NivelValido_RetornaValido(NivelPermissao nivel)
    {
        var request = new PermissaoUsuarioRequest { Modulo = "receitas", Nivel = nivel };

        var result = new PermissaoUsuarioRequestValidator().Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_NivelInvalido_RetornaInvalido()
    {
        var request = new PermissaoUsuarioRequest { Modulo = "receitas", Nivel = (NivelPermissao)99 };

        var result = new PermissaoUsuarioRequestValidator().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PermissaoUsuarioRequest.Nivel));
    }

    [Fact]
    public void Validar_ModuloVazio_RetornaInvalido()
    {
        var request = new PermissaoUsuarioRequest { Modulo = string.Empty, Nivel = NivelPermissao.Leitura };

        var result = new PermissaoUsuarioRequestValidator().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PermissaoUsuarioRequest.Modulo));
    }
}
