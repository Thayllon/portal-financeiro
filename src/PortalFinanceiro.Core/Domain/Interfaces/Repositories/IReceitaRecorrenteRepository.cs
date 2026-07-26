using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IReceitaRecorrenteRepository
{
    Task<ReceitaRecorrente?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<ReceitaRecorrente>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(ReceitaRecorrente entity);
    Task AtualizarAsync(ReceitaRecorrente entity);
}
