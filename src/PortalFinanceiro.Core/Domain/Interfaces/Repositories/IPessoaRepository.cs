using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IPessoaRepository
{
    Task<Pessoa?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Pessoa>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(Pessoa entity);
    Task AtualizarAsync(Pessoa entity);
}