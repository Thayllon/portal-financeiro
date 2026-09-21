namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ContratoRequest
{
    public string Nome { get; set; } = string.Empty;
    public Guid IdCliente { get; set; }
    public decimal Valor { get; set; }
}
