namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class ParceriaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid IdParceiro { get; set; }
    public string Parceiro { get; set; } = string.Empty;
    public Guid IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal PercentualParceiro { get; set; }
    public decimal ValorParceiro { get; set; }
    public decimal MinhaParte { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public decimal TotalRecebido { get; set; }
    public decimal TotalPago { get; set; }
    public decimal FaltaReceber { get; set; }
    public decimal FaltaPagar { get; set; }
}
