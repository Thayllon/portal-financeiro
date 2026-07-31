using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IDespesaRepository
{
    Task<Despesa?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Despesa>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, string? status = null, Guid? idCategoria = null, string? busca = null);
    Task InserirAsync(Despesa entity);
    Task InserirEmMassaAsync(IEnumerable<Despesa> entities);
    Task AtualizarAsync(Despesa entity);
    Task ExcluirAsync(Guid id);
}
