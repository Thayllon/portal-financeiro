using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IContaBancariaAppService
{
    Task<Result<IEnumerable<ContaBancariaResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<ContaBancariaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario);
    Task<Result<ContaBancariaResponse>> AdicionarAsync(Guid idUsuario, ContaBancariaRequest request);
    Task<Result<ContaBancariaResponse>> AtualizarAsync(Guid id, Guid idUsuario, ContaBancariaRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario);
}
