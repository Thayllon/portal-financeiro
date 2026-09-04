using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class CategoriaReceitaRepository : CategoriaRepositoryBase<CategoriaReceita>, ICategoriaReceitaRepository
{
    public CategoriaReceitaRepository(IDatabaseConnectionFactory connectionFactory)
        : base(connectionFactory, "CategoriaReceita") { }
}
