using FluentAssertions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Validations;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Validacao")]
public class ContratoRequestValidatorTests
{
    private static ContratoRequest RecorrenteValido() => new()
    {
        Nome = "Mensalidade",
        IdCliente = Guid.NewGuid(),
        Valor = 500,
        EhRecorrente = true,
        IdCategoria = Guid.NewGuid(),
        IdConta = Guid.NewGuid(),
        Dia = 5,
        DiaUtil = false,
        DataInicio = new DateTime(2026, 1, 1),
        DataFim = new DateTime(2026, 12, 31)
    };

    [Fact]
    public void Validar_SimplesValido_NaoAcusaRecorrencia()
    {
        var request = new ContratoRequest { Nome = "Aluguel", IdCliente = Guid.NewGuid(), Valor = 100 };

        var result = new ContratoRequestValidator().Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_RecorrenteSemCategoria_RetornaInvalido()
    {
        var request = RecorrenteValido();
        request.IdCategoria = null;

        var result = new ContratoRequestValidator().Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validar_RecorrenteSemConta_RetornaInvalido()
    {
        var request = RecorrenteValido();
        request.IdConta = null;

        var result = new ContratoRequestValidator().Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void Validar_RecorrenteDiaForaDoIntervalo_RetornaInvalido(int dia)
    {
        var request = RecorrenteValido();
        request.Dia = dia;

        var result = new ContratoRequestValidator().Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validar_RecorrenteDiaUtilComDia6_RetornaInvalido()
    {
        var request = RecorrenteValido();
        request.DiaUtil = true;
        request.Dia = 6;

        var result = new ContratoRequestValidator().Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validar_RecorrenteDataFimAnterior_RetornaInvalido()
    {
        var request = RecorrenteValido();
        request.DataFim = new DateTime(2025, 12, 31);

        var result = new ContratoRequestValidator().Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validar_RecorrenteValido_RetornaValido()
    {
        var result = new ContratoRequestValidator().Validate(RecorrenteValido());

        result.IsValid.Should().BeTrue();
    }
}
