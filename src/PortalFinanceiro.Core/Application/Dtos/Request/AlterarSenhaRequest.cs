namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class AlterarSenhaRequest
{
    public string Email { get; set; } = string.Empty;
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}