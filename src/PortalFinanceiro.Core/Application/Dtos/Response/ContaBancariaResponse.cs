using PortalFinanceiro.Core.Domain.Enums;

namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class ContaBancariaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}
