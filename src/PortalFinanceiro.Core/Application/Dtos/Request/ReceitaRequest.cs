namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ReceitaRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public Guid IdConta { get; set; }
    public Guid IdCategoria { get; set; }
    public Guid? IdSubcategoria { get; set; }
    public bool Repete { get; set; }
    public int? Dia { get; set; }
    public bool? DiaUtil { get; set; }
    public DateTime? DataFim { get; set; }
    public bool GeraDas { get; set; }
    public decimal? PercentualDas { get; set; }
}
