using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class UsuarioRepository : SqlBaseRepository, IUsuarioRepository
{
    public UsuarioRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Usuario?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Usuario>(conn, UsuarioSql.ObterPorId, new { Id = id }));

    public async Task<Usuario?> ObterPorEmailAsync(string email)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Usuario>(conn, UsuarioSql.ObterPorEmail, new { Email = email }));

    public async Task<IEnumerable<Usuario>> ListarAsync()
        => await ExecuteWithConnectionAsync(conn => QueryAsync<Usuario>(conn, UsuarioSql.Listar));

    public async Task InserirAsync(Usuario entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, UsuarioSql.Inserir, entity));

    public async Task AtualizarAsync(Usuario entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, UsuarioSql.Atualizar, entity));
}
