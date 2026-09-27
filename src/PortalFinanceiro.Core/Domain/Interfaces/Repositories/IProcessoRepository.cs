namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Projections;

public interface IProcessoRepository
{
    Task<Processo?> ObterPorIdAsync(Guid id);
    Task<ProcessoProjecao?> ObterProjecaoPorIdAsync(Guid id);
    Task<IEnumerable<ProcessoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null);
    Task InserirAsync(Processo entity);
    Task AtualizarAsync(Processo entity);
    Task<int> ContarAtivosPorParceriaAsync(Guid idParceria);
    Task<int> ContarAtivosPorContratoAsync(Guid idContrato);
    Task<ProcessoEtapa?> ObterEtapaPorIdAsync(Guid id);
    Task<IEnumerable<ProcessoEtapa>> ListarEtapasAsync(Guid idProcesso);
    Task<int> ProximaOrdemAsync(Guid idProcesso);
    Task<int> ContarEtapasPendentesAsync(Guid idProcesso);
    Task InserirEtapaAsync(ProcessoEtapa entity);
    Task AtualizarEtapaAsync(ProcessoEtapa entity);
    Task ExcluirEtapaAsync(Guid id);
}
