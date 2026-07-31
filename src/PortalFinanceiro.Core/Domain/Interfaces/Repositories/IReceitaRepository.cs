using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IReceitaRepository
{
    Task<Receita?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Receita>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, string? status = null, Guid? idCategoria = null, string? busca = null);
    Task InserirAsync(Receita entity);
    Task InserirEmMassaAsync(IEnumerable<Receita> entities);
    Task AtualizarAsync(Receita entity);
    Task ExcluirAsync(Guid id);
}
