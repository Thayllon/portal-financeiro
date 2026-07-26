using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IDespesaRecorrenteRepository
{
    Task<DespesaRecorrente?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<DespesaRecorrente>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(DespesaRecorrente entity);
    Task AtualizarAsync(DespesaRecorrente entity);
}
