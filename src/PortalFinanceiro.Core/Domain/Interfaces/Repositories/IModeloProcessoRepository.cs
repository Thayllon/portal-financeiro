namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Projections;

public interface IModeloProcessoRepository
{
    Task<ModeloProcesso?> ObterPorIdAsync(Guid id);
    Task<ModeloProcessoProjecao?> ObterProjecaoPorIdAsync(Guid id);
    Task<IEnumerable<ModeloProcessoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null);
    Task InserirAsync(ModeloProcesso entity);
    Task AtualizarAsync(ModeloProcesso entity);
    Task ExcluirAsync(Guid id);
    Task<ModeloEtapa?> ObterEtapaPorIdAsync(Guid id);
    Task<IEnumerable<ModeloEtapa>> ListarEtapasAsync(Guid idModeloProcesso);
    Task<int> ProximaOrdemEtapaAsync(Guid idModeloProcesso);
    Task InserirEtapaAsync(ModeloEtapa entity);
    Task AtualizarEtapaAsync(ModeloEtapa entity);
    Task ExcluirEtapaAsync(Guid id);
    Task<ModeloItem?> ObterItemPorIdAsync(Guid id);
    Task<IEnumerable<ModeloItem>> ListarItensAsync(Guid idModeloEtapa);
    Task<int> ProximaOrdemItemAsync(Guid idModeloEtapa);
    Task InserirItemAsync(ModeloItem entity);
    Task AtualizarItemAsync(ModeloItem entity);
    Task ExcluirItemAsync(Guid id);
}
