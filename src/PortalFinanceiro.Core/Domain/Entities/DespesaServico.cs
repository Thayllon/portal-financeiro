namespace PortalFinanceiro.Core.Domain.Entities;

public class DespesaServico
{
    public Guid Id { get; private set; }
    public Guid DespesaId { get; private set; }
    public Guid CategoriaServicoId { get; private set; }
    public Guid? SubcategoriaServicoId { get; private set; }

    public DespesaServico() { }

    public DespesaServico(Guid despesaId, Guid categoriaServicoId, Guid? subcategoriaServicoId = null)
    {
        Id = Guid.NewGuid();
        DespesaId = despesaId;
        CategoriaServicoId = categoriaServicoId;
        SubcategoriaServicoId = subcategoriaServicoId;
    }
}
