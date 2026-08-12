using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface ICategoriaDespesaAppService
{
    Task<Result<IEnumerable<CategoriaResponse>>> ListarAsync(Guid idUsuario, bool isAdmin);
    Task<Result<CategoriaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario, bool isAdmin);
    Task<Result<CategoriaResponse>> AdicionarAsync(Guid idUsuario, CategoriaRequest request);
    Task<Result<CategoriaResponse>> AtualizarAsync(Guid id, Guid idUsuario, bool isAdmin, CategoriaRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario, bool isAdmin);
}
