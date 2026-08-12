namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ProLaboreRequest
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public decimal Valor { get; set; }
    public decimal PercentualInss { get; set; }
    public Guid IdConta { get; set; }
}
