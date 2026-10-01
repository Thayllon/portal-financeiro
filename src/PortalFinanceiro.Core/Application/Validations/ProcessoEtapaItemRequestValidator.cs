using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class ProcessoEtapaItemRequestValidator : AbstractValidator<ProcessoEtapaItemRequest>
{
    public ProcessoEtapaItemRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}
