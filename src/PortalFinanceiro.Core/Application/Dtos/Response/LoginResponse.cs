namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public DateTime DataExpiracao { get; set; }
}
