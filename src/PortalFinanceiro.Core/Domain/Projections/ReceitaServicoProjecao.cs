namespace PortalFinanceiro.Core.Domain.Projections;

public class ReceitaServicoProjecao
{
    public Guid Id { get; set; }
    public Guid ReceitaId { get; set; }
    public Guid CategoriaServicoId { get; set; }
    public string CategoriaServico { get; set; } = string.Empty;
    public Guid? SubcategoriaServicoId { get; set; }
    public string SubcategoriaServico { get; set; } = string.Empty;
}
