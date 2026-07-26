using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IReceitaMensalRepository
{
    Task<ReceitaMensal?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<ReceitaMensal>> ListarPorMesAsync(Guid idUsuario, int mes, int ano);
    Task<IEnumerable<ReceitaMensal>> ListarPorReceitaRecorrenteAsync(Guid idReceitaRecorrente);
    Task InserirAsync(ReceitaMensal entity);
    Task InserirEmMassaAsync(IEnumerable<ReceitaMensal> entities);
    Task AtualizarAsync(ReceitaMensal entity);
}
