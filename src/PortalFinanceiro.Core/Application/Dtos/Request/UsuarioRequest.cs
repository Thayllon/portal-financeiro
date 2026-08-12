namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class UsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool Ativo { get; set; } = true;
}
