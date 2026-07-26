namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class DespesaMensalResponse
{
    public Guid Id { get; set; }
    public Guid IdDespesaRecorrente { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal Valor { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string Status { get; set; } = string.Empty;
}
