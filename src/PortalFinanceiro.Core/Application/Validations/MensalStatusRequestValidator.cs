using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class MensalStatusRequestValidator : AbstractValidator<MensalStatusRequest>
{
    public MensalStatusRequestValidator()
    {
        RuleFor(x => x.Data).NotEmpty();
    }
}
