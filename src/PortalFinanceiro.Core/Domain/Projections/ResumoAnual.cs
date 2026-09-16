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

public class ResumoAnualCategoriaItem
{
    public string Categoria { get; set; } = string.Empty;
    public string Subcategoria { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class ResumoParceriaAnual
{
    public decimal TotalRecebido { get; set; }
    public decimal TotalPago { get; set; }
    public decimal AReceber { get; set; }
    public decimal APagar { get; set; }
    public int QtdParcerias { get; set; }
}
