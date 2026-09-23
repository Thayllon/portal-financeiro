using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IContratoAppService
{
    Task<Result<IEnumerable<ContratoResponse>>> ListarAsync(Guid idUsuario, bool? ativo = null, bool? ehRecorrente = null);
    Task<Result<ContratoResponse>> ObterPorIdAsync(Guid id, Guid idUsuario);
    Task<Result<ContratoResponse>> AdicionarAsync(Guid idUsuario, ContratoRequest request);
    Task<Result<ContratoResponse>> AtualizarAsync(Guid id, Guid idUsuario, ContratoRequest request);
    Task<Result<Unit>> EncerrarAsync(Guid id, Guid idUsuario);
    Task<Result<Unit>> ReativarAsync(Guid id, Guid idUsuario);
    Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario);
}
