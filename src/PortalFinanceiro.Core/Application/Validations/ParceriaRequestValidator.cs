using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class ParceriaRequestValidator : AbstractValidator<ParceriaRequest>
{
    public ParceriaRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.IdParceiro).NotEmpty();
        RuleFor(x => x.IdCliente).NotEmpty();
        RuleFor(x => x.Valor).GreaterThan(0);
        RuleFor(x => x.PercentualParceiro).InclusiveBetween(0, 100);
    }
}
