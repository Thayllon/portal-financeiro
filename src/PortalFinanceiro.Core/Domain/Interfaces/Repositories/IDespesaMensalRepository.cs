using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IDespesaMensalRepository
{
    Task<DespesaMensal?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<DespesaMensal>> ListarPorMesAsync(Guid idUsuario, int mes, int ano);
    Task<IEnumerable<DespesaMensal>> ListarPorDespesaRecorrenteAsync(Guid idDespesaRecorrente);
    Task InserirAsync(DespesaMensal entity);
    Task InserirEmMassaAsync(IEnumerable<DespesaMensal> entities);
    Task AtualizarAsync(DespesaMensal entity);
}
