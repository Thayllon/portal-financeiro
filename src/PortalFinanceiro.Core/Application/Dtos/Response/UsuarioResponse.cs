namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class UsuarioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}
