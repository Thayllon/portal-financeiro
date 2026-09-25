using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Base;
using System.Data;

namespace PortalFinanceiro.Infrastructure.Repositories;

public class ContratoRepository : SqlBaseRepository, IContratoRepository
{
    public ContratoRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Contrato?> ObterPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<Contrato>(conn, ContratoSql.ObterPorId, new { Id = id }));

    public async Task<ContratoProjecao?> ObterProjecaoPorIdAsync(Guid id)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<ContratoProjecao>(conn, ContratoSql.ObterProjecaoPorId, new { Id = id }));

    public async Task<IEnumerable<ContratoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null, bool? ehRecorrente = null)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ContratoProjecao>(conn, ContratoSql.ListarPorUsuario, new { IdUsuario = idUsuario, Ativo = ativo, EhRecorrente = ehRecorrente }));

    public async Task<IEnumerable<ContratoProjecao>> ListarComTotaisAsync(Guid idUsuario, bool? ativo, bool? ehRecorrente, int statusRealizado)
        => await ExecuteWithConnectionAsync(conn => QueryAsync<ContratoProjecao>(conn, ContratoSql.ListarPorUsuarioComTotais, new { IdUsuario = idUsuario, Ativo = ativo, EhRecorrente = ehRecorrente, StatusRealizado = statusRealizado }));

    public async Task InserirAsync(Contrato entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ContratoSql.Inserir, entity));

    public async Task AtualizarAsync(Contrato entity)
        => await ExecuteWithConnectionAsync(conn => ExecuteAsync(conn, ContratoSql.Atualizar, entity));

    public async Task<decimal> SomarReceitasPorStatusAsync(Guid idContrato, int status)
        => await ExecuteWithConnectionAsync(conn => QueryFirstOrDefaultAsync<decimal>(conn, ContratoSql.SomarReceitas, new { IdContrato = idContrato, Status = status }));
}
