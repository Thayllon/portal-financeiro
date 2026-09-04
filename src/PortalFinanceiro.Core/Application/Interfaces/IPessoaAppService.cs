using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IPessoaAppService
{
    Task<Result<IEnumerable<PessoaResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<PessoaResponse>> ObterPorIdAsync(Guid id);
    Task<Result<PessoaResponse>> AdicionarAsync(Guid idUsuario, PessoaRequest request);
    Task<Result<PessoaResponse>> AtualizarAsync(Guid id, PessoaRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}