using PortalFinanceiro.Core.Domain.Enums;

namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ContaBancariaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
}
