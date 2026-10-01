namespace PortalFinanceiro.Core.Domain.Projections;

using PortalFinanceiro.Core.Domain.Enums;

public class ResumoAnualItem
{
    public int Mes { get; set; }
    public decimal Total { get; set; }
    public decimal TotalRealizado { get; set; }
    public decimal TotalRecorrente { get; set; }
}

public class ResumoAnualContaItem
{
    public string NomeConta { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
    public decimal Total { get; set; }
    public decimal TotalRealizado { get; set; }
}

public class ResumoRealizadoContaItem
{
    public Guid? IdConta { get; set; }
    public string NomeConta { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
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
