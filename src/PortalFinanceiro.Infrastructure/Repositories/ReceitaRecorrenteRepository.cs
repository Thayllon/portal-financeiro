using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ReceitaRecorrenteRepository : SqlBaseRepository, IReceitaRecorrenteRepository
{
    public ReceitaRecorrenteRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<ReceitaRecorrente?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ReceitaRecorrente>(conn, ReceitaRecorrenteSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<ReceitaRecorrente>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ReceitaRecorrente>(conn, ReceitaRecorrenteSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(ReceitaRecorrente entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaRecorrenteSql.Inserir, entity));

    public async Task AtualizarAsync(ReceitaRecorrente entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ReceitaRecorrenteSql.Atualizar, entity));
}
