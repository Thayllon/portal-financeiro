using PortalFinanceiro.Core.Domain.Results;

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

    public static Result<ReceitaServico> Criar(Guid receitaId, Guid categoriaServicoId, Guid? subcategoriaServicoId = null)
    {
        if (receitaId == Guid.Empty)
            return Erro.Validacao("RECEITA_OBRIGATORIA", "Receita é obrigatória.");
        if (categoriaServicoId == Guid.Empty)
            return Erro.Validacao("CATEGORIA_SERVICO_OBRIGATORIA", "Categoria de serviço é obrigatória.");

        return new ReceitaServico(receitaId, categoriaServicoId, subcategoriaServicoId);
    }
}
