using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<IEnumerable<Usuario>> ListarAsync();
    Task InserirAsync(Usuario entity);
    Task AtualizarAsync(Usuario entity);
}
