using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class PermissaoUsuarioRequestValidator : AbstractValidator<PermissaoUsuarioRequest>
{
    public PermissaoUsuarioRequestValidator()
    {
        RuleFor(x => x.Modulo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Nivel).IsInEnum();
    }
}
