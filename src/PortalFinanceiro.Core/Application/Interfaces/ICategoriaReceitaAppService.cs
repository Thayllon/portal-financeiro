using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface ICategoriaReceitaAppService
{
    Task<Result<IEnumerable<CategoriaResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<CategoriaResponse>> ObterPorIdAsync(Guid id);
    Task<Result<CategoriaResponse>> AdicionarAsync(Guid idUsuario, CategoriaRequest request);
    Task<Result<CategoriaResponse>> AtualizarAsync(Guid id, CategoriaRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}
