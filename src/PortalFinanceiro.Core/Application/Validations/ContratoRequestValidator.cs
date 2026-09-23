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

        When(x => x.EhRecorrente, () =>
        {
            RuleFor(x => x.IdCategoria).NotEmpty().WithMessage("Categoria é obrigatória para contrato recorrente.");
            RuleFor(x => x.IdConta).NotEmpty().WithMessage("Conta é obrigatória para contrato recorrente.");
            RuleFor(x => x.Dia).NotNull().InclusiveBetween(1, 31).WithMessage("Dia deve estar entre 1 e 31.");
            RuleFor(x => x.DataFim).NotNull().WithMessage("Data fim é obrigatória para contrato recorrente.")
                .Must((req, dataFim) => !req.DataInicio.HasValue || !dataFim.HasValue || dataFim.Value >= req.DataInicio.Value)
                .WithMessage("Data fim deve ser posterior à data início.");
        });

        When(x => x.EhRecorrente && x.DiaUtil == true, () =>
        {
            RuleFor(x => x.Dia).InclusiveBetween(1, 5).WithMessage("Dia útil deve estar entre 1 e 5.");
        });
    }
}
