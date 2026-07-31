using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IRegraDespesaAppService
{
    Task<Result<IEnumerable<RegraDespesaResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<RegraDespesaResponse>> ObterPorIdAsync(Guid id);
    Task<Result<RegraDespesaResponse>> AtualizarAsync(Guid id, RegraDespesaRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}
