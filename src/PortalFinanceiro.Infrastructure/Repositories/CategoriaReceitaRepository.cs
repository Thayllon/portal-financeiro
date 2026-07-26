using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class CategoriaReceitaRepository : SqlBaseRepository, ICategoriaReceitaRepository
{
    public CategoriaReceitaRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<CategoriaReceita?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<CategoriaReceita>(conn, CategoriaReceitaSql.ObterPorId, new { Id = id }));

    public async Task<IEnumerable<CategoriaReceita>> ListarPorUsuarioAsync(Guid idUsuario)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<CategoriaReceita>(conn, CategoriaReceitaSql.ListarPorUsuario, new { IdUsuario = idUsuario }));

    public async Task InserirAsync(CategoriaReceita entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaReceitaSql.Inserir, entity));

    public async Task AtualizarAsync(CategoriaReceita entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaReceitaSql.Atualizar, entity));
}
