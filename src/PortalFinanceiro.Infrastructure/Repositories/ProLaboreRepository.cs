using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ProLaboreRepository : SqlBaseRepository, IProLaboreRepository
{
    public ProLaboreRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<ProLabore?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ProLabore>(conn, ProLaboreSql.ObterPorId, new { Id = id }));

    public async Task<ProLabore?> ObterPorUsuarioMesAsync(Guid idUsuario, int mes, int ano)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ProLabore>(conn, ProLaboreSql.ObterPorMes, new { IdUsuario = idUsuario, Mes = mes, Ano = ano }));

    public async Task<IEnumerable<ProLabore>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ProLabore>(conn, ProLaboreSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(ProLabore entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProLaboreSql.Inserir, entity));

    public async Task AtualizarAsync(ProLabore entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProLaboreSql.Atualizar, entity));

    public async Task ExcluirAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ProLaboreSql.Excluir, new { Id = id }));
}