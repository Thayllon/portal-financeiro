using FluentAssertions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Validations;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Validacao")]
public class ReceitaRequestValidatorTests
{
    private static ReceitaRequest RequisicaoValida() => new()
    {
        Descricao = "Salário",
        Valor = 1000,
        Data = new DateTime(2026, 1, 5),
        IdConta = Guid.NewGuid(),
        IdCategoria = Guid.NewGuid()
    };

    [Fact]
    public void Validar_ComParceriaEContrato_RetornaVinculoDuplo()
    {
        var request = RequisicaoValida();
        request.IdParceria = Guid.NewGuid();
        request.IdContrato = Guid.NewGuid();

        var result = new ReceitaRequestValidator().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Informe parceria ou contrato, nunca os dois.");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Validar_SemVinculoDuplo_NaoAcusaVinculoDuplo(bool comParceria, bool comContrato)
    {
        var request = RequisicaoValida();
        if (comParceria) request.IdParceria = Guid.NewGuid();
        if (comContrato) request.IdContrato = Guid.NewGuid();

        var result = new ReceitaRequestValidator().Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
