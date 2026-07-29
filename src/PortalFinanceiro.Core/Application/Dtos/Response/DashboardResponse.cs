namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class DashboardResponse
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalRecebido { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalPago { get; set; }
    public decimal Saldo { get; set; }
    public decimal SaldoRealizado { get; set; }
    public List<ResumoPorConta> ResumoPorConta { get; set; } = [];
    public List<PrevisaoMensal> PrevisaoProximosMeses { get; set; } = [];
}

public class ResumoPorConta
{
    public string NomeConta { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Saldo { get; set; }
}

public class PrevisaoMensal
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal SaldoPrevisto { get; set; }
}
