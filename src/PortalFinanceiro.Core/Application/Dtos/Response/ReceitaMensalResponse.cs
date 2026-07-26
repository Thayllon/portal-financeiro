namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class ReceitaMensalResponse
{
    public Guid Id { get; set; }
    public Guid IdReceitaRecorrente { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal Valor { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public string Status { get; set; } = string.Empty;
}
