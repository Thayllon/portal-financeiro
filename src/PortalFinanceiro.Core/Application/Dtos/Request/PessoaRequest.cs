using PortalFinanceiro.Core.Domain.Enums;

namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class PessoaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public TipoPessoa Tipo { get; set; }
}