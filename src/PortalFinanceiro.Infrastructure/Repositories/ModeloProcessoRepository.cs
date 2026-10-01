using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ModeloProcessoRepository : SqlBaseRepository, IModeloProcessoRepository
{
    public ModeloProcessoRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<ModeloProcesso?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ModeloProcesso>(conn, ModeloProcessoSql.ObterPorId, new { Id = id }));

    public async Task<ModeloProcessoProjecao?> ObterProjecaoPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ModeloProcessoProjecao>(conn, ModeloProcessoSql.ObterProjecaoPorId, new { Id = id }));

    public async Task<IEnumerable<ModeloProcessoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ModeloProcessoProjecao>(conn, ModeloProcessoSql.ListarPorUsuario, new { IdUsuario = idUsuario, Ativo = ativo }));

    public async Task InserirAsync(ModeloProcesso entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.Inserir, entity));

    public async Task AtualizarAsync(ModeloProcesso entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.Atualizar, entity));

    public async Task ExcluirAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.Excluir, new { Id = id }));

    public async Task<ModeloEtapa?> ObterEtapaPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ModeloEtapa>(conn, ModeloProcessoSql.ObterEtapaPorId, new { Id = id }));

    public async Task<IEnumerable<ModeloEtapa>> ListarEtapasAsync(Guid idModeloProcesso)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ModeloEtapa>(conn, ModeloProcessoSql.ListarEtapas, new { IdModeloProcesso = idModeloProcesso }));

    public async Task<int> ProximaOrdemEtapaAsync(Guid idModeloProcesso)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ModeloProcessoSql.ProximaOrdemEtapa, new { IdModeloProcesso = idModeloProcesso }));

    public async Task InserirEtapaAsync(ModeloEtapa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.InserirEtapa, entity));

    public async Task AtualizarEtapaAsync(ModeloEtapa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.AtualizarEtapa, entity));

    public async Task ExcluirEtapaAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.ExcluirEtapa, new { Id = id }));

    public async Task<ModeloItem?> ObterItemPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ModeloItem>(conn, ModeloProcessoSql.ObterItemPorId, new { Id = id }));

    public async Task<IEnumerable<ModeloItem>> ListarItensAsync(Guid idModeloEtapa)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ModeloItem>(conn, ModeloProcessoSql.ListarItens, new { IdModeloEtapa = idModeloEtapa }));

    public async Task<int> ProximaOrdemItemAsync(Guid idModeloEtapa)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<int>(conn, ModeloProcessoSql.ProximaOrdemItem, new { IdModeloEtapa = idModeloEtapa }));

    public async Task InserirItemAsync(ModeloItem entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.InserirItem, entity));

    public async Task AtualizarItemAsync(ModeloItem entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.AtualizarItem, entity));

    public async Task ExcluirItemAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ModeloProcessoSql.ExcluirItem, new { Id = id }));
}
