using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class ContaBancariaRequestValidator : AbstractValidator<ContaBancariaRequest>
{
    public ContaBancariaRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Banco).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Tipo).IsInEnum();
    }
}
