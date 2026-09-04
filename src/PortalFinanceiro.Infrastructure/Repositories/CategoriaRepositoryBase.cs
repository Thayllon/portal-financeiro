using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;

namespace PortalFinanceiro.Infrastructure.Repositories;

public abstract class CategoriaRepositoryBase<T> : SqlBaseRepository, ICategoriaRepository<T> where T : class
{
    protected readonly string Tabela;

    protected CategoriaRepositoryBase(IDatabaseConnectionFactory connectionFactory, string tabela) : base(connectionFactory)
    {
        Tabela = tabela;
    }

    public async Task<T?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<T>(conn, CategoriaSql.ObterPorId(Tabela), new { Id = id }));

    public async Task<IEnumerable<T>> ListarAsync()
        => await ExecuteWithConnectionAsync(conn => QueryAsync<T>(conn, CategoriaSql.ListarAtivas(Tabela)));

    public async Task<IEnumerable<T>> ListarPorPaiAsync(Guid? categoriaPaiId)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<T>(conn, CategoriaSql.ListarPorPai(Tabela), new { CategoriaPaiId = categoriaPaiId }));

    public async Task InserirAsync(T entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaSql.Inserir(Tabela), entity));

    public async Task AtualizarAsync(T entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaSql.Atualizar(Tabela), entity));

    public async Task ExcluirAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, CategoriaSql.Excluir(Tabela), new { Id = id }));
}
