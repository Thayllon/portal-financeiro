namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ParceriaRequest
{
    public string Nome { get; set; } = string.Empty;
    public Guid IdParceiro { get; set; }
    public Guid IdCliente { get; set; }
    public decimal Valor { get; set; }
    public decimal PercentualParceiro { get; set; }
}
