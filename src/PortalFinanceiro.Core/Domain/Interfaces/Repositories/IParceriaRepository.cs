using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IParceriaRepository
{
    Task<Parceria?> ObterPorIdAsync(Guid id);
    Task<ParceriaProjecao?> ObterProjecaoPorIdAsync(Guid id);
    Task<IEnumerable<ParceriaProjecao>> ListarAsync(Guid idUsuario);
    Task InserirAsync(Parceria entity);
    Task AtualizarAsync(Parceria entity);
    Task<decimal> SomarReceitasPorStatusAsync(Guid idParceria, int status);
    Task<decimal> SomarDespesasPorStatusAsync(Guid idParceria, int status);
    Task<ResumoParceriaAnual> ResumoAnualAsync(Guid idUsuario, int ano, Guid? idConta = null);
}
