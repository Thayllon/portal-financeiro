using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IUsuarioAppService
{
    Task<Result<IEnumerable<UsuarioResponse>>> ListarAsync();
    Task<Result<UsuarioResponse>> AdicionarAsync(UsuarioRequest request);
    Task<Result<UsuarioResponse>> AtualizarAsync(Guid id, UsuarioRequest request);
    Task<Result<Unit>> AlterarAtivoAsync(Guid id, bool ativo);
    Task<Result<Unit>> ResetarSenhaAsync(Guid id);
}
