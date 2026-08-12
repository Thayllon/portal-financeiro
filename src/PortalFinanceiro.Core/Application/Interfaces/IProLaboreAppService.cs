using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IProLaboreAppService
{
    Task<Result<IEnumerable<ProLaboreResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<ProLaboreResponse>> AdicionarAsync(Guid idUsuario, ProLaboreRequest request);
    Task<Result<ProLaboreResponse>> AtualizarAsync(Guid id, Guid idUsuario, ProLaboreRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}
