namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class ReceitaResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public Guid IdConta { get; set; }
    public string Conta { get; set; } = string.Empty;
    public Guid IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public Guid? IdSubcategoria { get; set; }
    public string Subcategoria { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DataRealizacao { get; set; }
    public Guid? IdRegra { get; set; }
    public bool EhRecorrente { get; set; }
    public bool GeraDas { get; set; }
    public decimal? PercentualDas { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}
