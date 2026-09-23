namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ContratoRequest
{
    public string Nome { get; set; } = string.Empty;
    public Guid IdCliente { get; set; }
    public decimal Valor { get; set; }
    public bool EhRecorrente { get; set; }
    public Guid? IdCategoria { get; set; }
    public Guid? IdSubcategoria { get; set; }
    public Guid? IdConta { get; set; }
    public int? Dia { get; set; }
    public bool? DiaUtil { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}
