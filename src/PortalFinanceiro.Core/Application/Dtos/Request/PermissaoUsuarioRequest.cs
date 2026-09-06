namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class PermissaoUsuarioRequest
{
    public string Modulo { get; set; } = string.Empty;
    public int Nivel { get; set; }
}
