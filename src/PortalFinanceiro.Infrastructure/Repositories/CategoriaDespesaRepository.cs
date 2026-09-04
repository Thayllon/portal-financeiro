using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class CategoriaDespesaRepository : CategoriaRepositoryBase<CategoriaDespesa>, ICategoriaDespesaRepository
{
    public CategoriaDespesaRepository(IDatabaseConnectionFactory connectionFactory)
        : base(connectionFactory, "CategoriaDespesa") { }
}
