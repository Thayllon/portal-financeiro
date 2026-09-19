using PortalFinanceiro.Core.Domain.Results;

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

    public static Result<DespesaServico> Criar(Guid despesaId, Guid categoriaServicoId, Guid? subcategoriaServicoId = null)
    {
        if (despesaId == Guid.Empty)
            return Erro.Validacao("DESPESA_OBRIGATORIA", "Despesa é obrigatória.");
        if (categoriaServicoId == Guid.Empty)
            return Erro.Validacao("CATEGORIA_SERVICO_OBRIGATORIA", "Categoria de serviço é obrigatória.");

        return new DespesaServico(despesaId, categoriaServicoId, subcategoriaServicoId);
    }
}
