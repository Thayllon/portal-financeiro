using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class PessoaRepository : SqlBaseRepository, IPessoaRepository
{
    public PessoaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Pessoa?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Pessoa>(conn, PessoaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<Pessoa>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<Pessoa>(conn, PessoaSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(Pessoa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, PessoaSql.Inserir, entity));

    public async Task AtualizarAsync(Pessoa entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, PessoaSql.Atualizar, entity));
}