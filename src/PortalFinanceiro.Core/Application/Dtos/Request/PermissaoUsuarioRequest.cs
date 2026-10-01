namespace PortalFinanceiro.Core.Application.Dtos.Request;

using PortalFinanceiro.Core.Domain.Entities;

public class PermissaoUsuarioRequest
{
    public string Modulo { get; set; } = string.Empty;
    public NivelPermissao Nivel { get; set; }
}
