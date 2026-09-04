using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class PessoaRequestValidator : AbstractValidator<PessoaRequest>
{
    public PessoaRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Telefone).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Telefone));
        RuleFor(x => x.Tipo).IsInEnum();
    }
}