using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class CategoriaServicoRepository : CategoriaRepositoryBase<CategoriaServico>, ICategoriaServicoRepository
{
    public CategoriaServicoRepository(IDatabaseConnectionFactory connectionFactory)
        : base(connectionFactory, "CategoriaServico") { }
}
