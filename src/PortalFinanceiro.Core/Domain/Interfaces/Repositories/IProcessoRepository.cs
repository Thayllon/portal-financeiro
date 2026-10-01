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
    Task ExcluirProcessoAsync(Guid id);
    Task<int> ContarAtivosPorParceriaAsync(Guid idParceria);
    Task<int> ContarAtivosPorContratoAsync(Guid idContrato);
    Task<int> ContarPorParceriaAsync(Guid idParceria);
    Task<int> ContarPorContratoAsync(Guid idContrato);
    Task<ProcessoEtapa?> ObterEtapaPorIdAsync(Guid id);
    Task<IEnumerable<ProcessoEtapa>> ListarEtapasAsync(Guid idProcesso);
    Task<int> ProximaOrdemAsync(Guid idProcesso);
    Task<int> ContarEtapasPendentesAsync(Guid idProcesso);
    Task InserirEtapaAsync(ProcessoEtapa entity);
    Task AtualizarEtapaAsync(ProcessoEtapa entity);
    Task ExcluirEtapaAsync(Guid id);
    Task<ProcessoEtapaItem?> ObterItemPorIdAsync(Guid id);
    Task<IEnumerable<ProcessoEtapaItem>> ListarItensAsync(Guid idProcessoEtapa);
    Task<int> ProximaOrdemItemAsync(Guid idProcessoEtapa);
    Task<int> ContarItensObrigatoriosPendentesAsync(Guid idProcessoEtapa);
    Task<int> ContarAnexosPorItemAsync(Guid idProcessoEtapaItem);
    Task InserirItemAsync(ProcessoEtapaItem entity);
    Task AtualizarItemAsync(ProcessoEtapaItem entity);
    Task ExcluirItemAsync(Guid id);
}
