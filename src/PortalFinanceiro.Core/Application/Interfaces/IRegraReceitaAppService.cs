using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IRegraReceitaAppService
{
    Task<Result<IEnumerable<RegraReceitaResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<RegraReceitaResponse>> ObterPorIdAsync(Guid id);
    Task<Result<RegraReceitaResponse>> AtualizarAsync(Guid id, RegraReceitaRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}
