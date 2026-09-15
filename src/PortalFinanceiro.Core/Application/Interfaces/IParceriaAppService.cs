using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IParceriaAppService
{
    Task<Result<IEnumerable<ParceriaResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<ParceriaResponse>> ObterPorIdAsync(Guid id);
    Task<Result<ParceriaResponse>> AdicionarAsync(Guid idUsuario, ParceriaRequest request);
    Task<Result<ParceriaResponse>> AtualizarAsync(Guid id, ParceriaRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}
