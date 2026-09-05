using FluentValidation;
using PortalFinanceiro.Core.Application.Dtos.Request;

namespace PortalFinanceiro.Core.Application.Validations;

public class UsuarioRequestValidator : AbstractValidator<UsuarioRequest>
{
    public UsuarioRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        When(x => !string.IsNullOrWhiteSpace(x.Senha), () =>
        {
            RuleFor(x => x.Senha).MinimumLength(6).MaximumLength(100);
        });
    }
}
