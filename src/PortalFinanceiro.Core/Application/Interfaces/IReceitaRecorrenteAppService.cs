using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IReceitaRecorrenteAppService
{
    Task<Result<IEnumerable<ReceitaRecorrenteResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<ReceitaRecorrenteResponse>> ObterPorIdAsync(Guid id);
    Task<Result<ReceitaRecorrenteResponse>> AdicionarAsync(Guid idUsuario, ReceitaRecorrenteRequest request);
    Task<Result<ReceitaRecorrenteResponse>> AtualizarAsync(Guid id, ReceitaRecorrenteRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}
