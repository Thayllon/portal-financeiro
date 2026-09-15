namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class DespesaResponse
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
    public int Status { get; set; }
    public DateTime? DataRealizacao { get; set; }
    public Guid? IdRegra { get; set; }
    public bool EhRecorrente { get; set; }
    public Guid? IdReceitaOrigem { get; set; }
    public Guid? IdParceria { get; set; }
    public string Parceria { get; set; } = string.Empty;
    public decimal? ParceriaValor { get; set; }
    public Guid? IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public List<DespesaServicoResponse> Servicos { get; set; } = new();
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class DespesaServicoResponse
{
    public Guid Id { get; set; }
    public Guid CategoriaServicoId { get; set; }
    public string CategoriaServico { get; set; } = string.Empty;
    public Guid? SubcategoriaServicoId { get; set; }
    public string SubcategoriaServico { get; set; } = string.Empty;
}
