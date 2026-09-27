using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class ProcessoRequestValidator : AbstractValidator<ProcessoRequest>
{
    public ProcessoRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}

public class ProcessoEtapaRequestValidator : AbstractValidator<ProcessoEtapaRequest>
{
    public ProcessoEtapaRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}
