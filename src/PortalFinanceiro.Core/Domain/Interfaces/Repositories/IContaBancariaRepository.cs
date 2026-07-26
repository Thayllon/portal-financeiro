using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IContaBancariaRepository
{
    Task<ContaBancaria?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<ContaBancaria>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(ContaBancaria entity);
    Task AtualizarAsync(ContaBancaria entity);
}
