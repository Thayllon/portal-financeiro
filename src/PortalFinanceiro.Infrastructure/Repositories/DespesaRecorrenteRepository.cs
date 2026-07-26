using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class DespesaRecorrenteRepository : SqlBaseRepository, IDespesaRecorrenteRepository
{
    public DespesaRecorrenteRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<DespesaRecorrente?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<DespesaRecorrente>(conn, DespesaRecorrenteSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<DespesaRecorrente>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<DespesaRecorrente>(conn, DespesaRecorrenteSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(DespesaRecorrente entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, DespesaRecorrenteSql.Inserir, entity));

    public async Task AtualizarAsync(DespesaRecorrente entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, DespesaRecorrenteSql.Atualizar, entity));
}
