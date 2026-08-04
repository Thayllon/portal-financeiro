using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IDespesaRepository
{
    Task<Despesa?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Despesa>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null);
    Task<int> ContarPorCategoriaAsync(Guid idCategoria);
    Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria);
    Task<int> ContarPorRegraAsync(Guid idRegra);
    Task<IEnumerable<Despesa>> ListarPorRegraAsync(Guid idRegra);
    Task InserirAsync(Despesa entity);
    Task InserirEmMassaAsync(IEnumerable<Despesa> entities);
    Task AtualizarAsync(Despesa entity);
    Task ExcluirAsync(Guid id);
}
