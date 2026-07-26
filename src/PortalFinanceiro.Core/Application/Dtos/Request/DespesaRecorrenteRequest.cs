namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class DespesaRecorrenteRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Dia { get; set; }
    public Guid IdCategoria { get; set; }
    public Guid IdConta { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}
