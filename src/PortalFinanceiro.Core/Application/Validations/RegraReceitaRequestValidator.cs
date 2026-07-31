using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class RegraReceitaRequestValidator : AbstractValidator<RegraReceitaRequest>
{
    public RegraReceitaRequestValidator()
    {
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Valor).GreaterThan(0);
        RuleFor(x => x.Dia).InclusiveBetween(1, 31);
        RuleFor(x => x.IdCategoria).NotEmpty();
        RuleFor(x => x.IdConta).NotEmpty();
        RuleFor(x => x.DataInicio).NotEmpty();
        RuleFor(x => x.DataFim).NotEmpty();
    }
}
