using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface ICategoriaHistoricoRepository
{
    Task InserirAsync(CategoriaHistorico entity);
    Task<IEnumerable<CategoriaHistorico>> ListarPorCategoriaAsync(Guid idCategoria, ETipoCategoria tipoCategoria);
}
