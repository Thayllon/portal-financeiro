namespace PortalFinanceiro.Core.Domain.Entities;

public class ReceitaServico
{
    public Guid Id { get; private set; }
    public Guid ReceitaId { get; private set; }
    public Guid CategoriaServicoId { get; private set; }
    public Guid? SubcategoriaServicoId { get; private set; }

    public ReceitaServico() { }

    public ReceitaServico(Guid receitaId, Guid categoriaServicoId, Guid? subcategoriaServicoId = null)
    {
        Id = Guid.NewGuid();
        ReceitaId = receitaId;
        CategoriaServicoId = categoriaServicoId;
        SubcategoriaServicoId = subcategoriaServicoId;
    }
}
