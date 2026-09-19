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
    public List<ResumoPorCategoria> ResumoPorCategoria { get; set; } = [];
    public List<PrevisaoMensal> PrevisaoProximosMeses { get; set; } = [];
}

public class DashboardAnualResponse
{
    public int Ano { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalRecebido { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalPago { get; set; }
    public decimal Saldo { get; set; }
    public decimal SaldoRealizado { get; set; }
    public decimal? VariacaoReceitasPercentual { get; set; }
    public decimal? VariacaoDespesasPercentual { get; set; }
    public decimal? VariacaoSaldoPercentual { get; set; }
    public decimal MediaMensalSaldo { get; set; }
    public int MesesConsiderados { get; set; }
    public List<MensalResumoAnual> ResumoPorMes { get; set; } = [];
    public List<ResumoPorContaAnual> ResumoPorConta { get; set; } = [];
    public List<DistribuicaoCategoriaAnual> DistribuicaoReceitas { get; set; } = [];
    public List<DistribuicaoCategoriaAnual> DistribuicaoDespesas { get; set; } = [];
    public List<PrevisaoMensal> PrevisaoRestanteAno { get; set; } = [];
    public ResumoParceriasAnual ResumoParcerias { get; set; } = new();
}

public class MensalResumoAnual
{
    public int Mes { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalRecebido { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalPago { get; set; }
    public decimal Saldo { get; set; }
    public decimal SaldoRealizado { get; set; }
}

public class ResumoPorContaAnual
{
    public string NomeConta { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal TotalReceitas { get; set; }
    public decimal TotalRecebido { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalPago { get; set; }
    public decimal Saldo { get; set; }
    public decimal SaldoRealizado { get; set; }
}

public class ResumoPorCategoria
{
    public string Nome { get; set; } = string.Empty;
    public decimal Total { get; set; }
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

public class DistribuicaoCategoriaAnual
{
    public string Nome { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Percentual { get; set; }
    public List<DistribuicaoCategoriaAnual> Subcategorias { get; set; } = [];
}

public class ResumoParceriasAnual
{
    public decimal TotalRecebido { get; set; }
    public decimal TotalPago { get; set; }
    public decimal AReceber { get; set; }
    public decimal APagar { get; set; }
    public decimal Saldo => TotalRecebido - TotalPago;
    public int QtdParcerias { get; set; }
}
