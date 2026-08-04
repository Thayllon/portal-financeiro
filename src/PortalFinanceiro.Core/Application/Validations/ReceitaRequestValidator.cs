using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class ReceitaRequestValidator : AbstractValidator<ReceitaRequest>
{
    public ReceitaRequestValidator()
    {
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Valor).GreaterThan(0);
        RuleFor(x => x.Data).NotEmpty();
        RuleFor(x => x.IdConta).NotEmpty();
        RuleFor(x => x.IdCategoria).NotEmpty();

        When(x => x.Repete, () =>
        {
            RuleFor(x => x.DataFim).NotNull();
            RuleFor(x => x.Dia).NotNull().InclusiveBetween(1, 31);

            When(x => x.DiaUtil == true, () =>
            {
                RuleFor(x => x.Dia).InclusiveBetween(1, 5);
            });
        });
    }
}
