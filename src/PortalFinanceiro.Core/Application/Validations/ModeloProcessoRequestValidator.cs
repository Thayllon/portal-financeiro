using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class ModeloProcessoRequestValidator : AbstractValidator<ModeloProcessoRequest>
{
    public ModeloProcessoRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}

public class ModeloEtapaRequestValidator : AbstractValidator<ModeloEtapaRequest>
{
    public ModeloEtapaRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}

public class ModeloItemRequestValidator : AbstractValidator<ModeloItemRequest>
{
    public ModeloItemRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}
