using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IReceitaRepository
{
    Task<Receita?> ObterPorIdAsync(Guid id);
    Task<ReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id);
    Task<IEnumerable<ReceitaProjecao>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null);
    Task<int> ContarPorCategoriaAsync(Guid idCategoria);
    Task<int> ContarPorSubcategoriaAsync(Guid idSubcategoria);
    Task<int> ContarPorRegraAsync(Guid idRegra);
    Task<IEnumerable<Receita>> ListarPorRegraAsync(Guid idRegra);
    Task InserirAsync(Receita entity);
    Task InserirEmMassaAsync(IEnumerable<Receita> entities);
    Task AtualizarAsync(Receita entity);
    Task ExcluirAsync(Guid id);
    Task<IEnumerable<ResumoAnualItem>> ResumoAnualPorMesAsync(Guid idUsuario, int ano, Guid? idConta = null);
    Task<IEnumerable<ResumoAnualContaItem>> ResumoAnualPorContaAsync(Guid idUsuario, int ano);
}