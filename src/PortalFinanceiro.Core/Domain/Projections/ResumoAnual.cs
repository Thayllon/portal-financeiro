namespace PortalFinanceiro.Core.Domain.Projections;

public class ResumoAnualItem
{
    public int Mes { get; set; }
    public decimal Total { get; set; }
    public decimal TotalRealizado { get; set; }
}

public class ResumoAnualContaItem
{
    public string NomeConta { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal TotalRealizado { get; set; }
}
