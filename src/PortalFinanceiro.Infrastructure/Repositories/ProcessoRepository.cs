using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ProcessoRepository : SqlBaseRepository, IProcessoRepository
{
    public ProcessoRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Processo?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Processo>(conn, ProcessoSql.ObterPorId, new { Id = id }));

    public async Task<ProcessoProjecao?> ObterProjecaoPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ProcessoProjecao>(conn, ProcessoSql.ObterProjecaoPorId, new { Id = id }));

    public async Task<IEnumerable<ProcessoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ProcessoProjecao>(conn, ProcessoSql.ListarPorUsuario, new { IdUsuario = idUsuario, Ativo = ativo }));

    public async Task InserirAsync(Processo entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.Inserir, entity));

    public async Task AtualizarAsync(Processo entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.Atualizar, entity));

    public async Task ExcluirProcessoAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.ExcluirProcesso, new { Id = id }));

    public async Task<int> ContarAtivosPorParceriaAsync(Guid idParceria)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ContarAtivosPorParceria, new { IdParceria = idParceria }));

    public async Task<int> ContarAtivosPorContratoAsync(Guid idContrato)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ContarAtivosPorContrato, new { IdContrato = idContrato }));

    public async Task<int> ContarPorParceriaAsync(Guid idParceria)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ContarPorParceria, new { IdParceria = idParceria }));

    public async Task<int> ContarPorContratoAsync(Guid idContrato)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ContarPorContrato, new { IdContrato = idContrato }));

    public async Task<ProcessoEtapa?> ObterEtapaPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ProcessoEtapa>(conn, ProcessoSql.ObterEtapaPorId, new { Id = id }));

    public async Task<IEnumerable<ProcessoEtapa>> ListarEtapasAsync(Guid idProcesso)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ProcessoEtapa>(conn, ProcessoSql.ListarEtapas, new { IdProcesso = idProcesso }));

    public async Task<int> ProximaOrdemAsync(Guid idProcesso)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ProximaOrdem, new { IdProcesso = idProcesso }));

    public async Task<int> ContarEtapasPendentesAsync(Guid idProcesso)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ContarEtapasPendentes, new { IdProcesso = idProcesso }));

    public async Task InserirEtapaAsync(ProcessoEtapa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.InserirEtapa, entity));

    public async Task AtualizarEtapaAsync(ProcessoEtapa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.AtualizarEtapa, entity));

    public async Task ExcluirEtapaAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.ExcluirEtapa, new { Id = id }));

    public async Task<ProcessoEtapaItem?> ObterItemPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ProcessoEtapaItem>(conn, ProcessoSql.ObterItemPorId, new { Id = id }));

    public async Task<IEnumerable<ProcessoEtapaItem>> ListarItensAsync(Guid idProcessoEtapa)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ProcessoEtapaItem>(conn, ProcessoSql.ListarItens, new { IdProcessoEtapa = idProcessoEtapa }));

    public async Task<int> ProximaOrdemItemAsync(Guid idProcessoEtapa)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ProximaOrdemItem, new { IdProcessoEtapa = idProcessoEtapa }));

    public async Task<int> ContarItensObrigatoriosPendentesAsync(Guid idProcessoEtapa)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ContarItensObrigatoriosPendentes, new { IdProcessoEtapa = idProcessoEtapa }));

    public async Task<int> ContarAnexosPorItemAsync(Guid idProcessoEtapaItem)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ProcessoSql.ContarAnexosPorItem, new { IdProcessoEtapaItem = idProcessoEtapaItem }));

    public async Task InserirItemAsync(ProcessoEtapaItem entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.InserirItem, entity));

    public async Task AtualizarItemAsync(ProcessoEtapaItem entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.AtualizarItem, entity));

    public async Task ExcluirItemAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProcessoSql.ExcluirItem, new { Id = id }));
}
