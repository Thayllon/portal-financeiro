using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class ContratoRequestValidator : AbstractValidator<ContratoRequest>
{
    public ContratoRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.IdCliente).NotEmpty();
        RuleFor(x => x.Valor).GreaterThan(0);
    }
}
