using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface ICategoriaReceitaRepository
{
    Task<CategoriaReceita?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<CategoriaReceita>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(CategoriaReceita entity);
    Task AtualizarAsync(CategoriaReceita entity);
}
