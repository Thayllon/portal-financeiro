using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ContaBancariaRepository : SqlBaseRepository, IContaBancariaRepository
{
    public ContaBancariaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<ContaBancaria?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ContaBancaria>(conn, ContaBancariaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<ContaBancaria>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ContaBancaria>(conn, ContaBancariaSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(ContaBancaria entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ContaBancariaSql.Inserir, entity));

    public async Task AtualizarAsync(ContaBancaria entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ContaBancariaSql.Atualizar, entity));
}
