namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ParceriaRequest
{
    public Guid IdParceiro { get; set; }
    public Guid IdCliente { get; set; }
    public decimal Valor { get; set; }
}
