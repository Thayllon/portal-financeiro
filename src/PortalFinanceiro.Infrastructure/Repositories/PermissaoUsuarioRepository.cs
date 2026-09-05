using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class PermissaoUsuarioRepository : SqlBaseRepository, IPermissaoUsuarioRepository
{
    public PermissaoUsuarioRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<PermissaoUsuario>> ObterPorUsuarioIdAsync(Guid usuarioId)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<PermissaoUsuario>(conn, PermissaoUsuarioSql.ObterPorUsuarioId, new { UsuarioId = usuarioId }));

    public async Task<PermissaoUsuario?> ObterPorUsuarioEModuloAsync(Guid usuarioId, string modulo)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<PermissaoUsuario>(conn, PermissaoUsuarioSql.ObterPorUsuarioEModulo, new { UsuarioId = usuarioId, Modulo = modulo }));

    public async Task InserirAsync(PermissaoUsuario entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, PermissaoUsuarioSql.Inserir, entity));

    public async Task AtualizarAsync(PermissaoUsuario entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, PermissaoUsuarioSql.Atualizar, entity));

    public async Task ExcluirAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, PermissaoUsuarioSql.Excluir, new { Id = id }));

    public async Task ExcluirPorUsuarioIdAsync(Guid usuarioId)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, PermissaoUsuarioSql.ExcluirPorUsuarioId, new { UsuarioId = usuarioId }));
}
