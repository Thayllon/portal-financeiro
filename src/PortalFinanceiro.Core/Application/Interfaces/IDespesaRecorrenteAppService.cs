using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IDespesaRecorrenteAppService
{
    Task<Result<IEnumerable<DespesaRecorrenteResponse>>> ListarAsync(Guid idUsuario);
    Task<Result<DespesaRecorrenteResponse>> ObterPorIdAsync(Guid id);
    Task<Result<DespesaRecorrenteResponse>> AdicionarAsync(Guid idUsuario, DespesaRecorrenteRequest request);
    Task<Result<DespesaRecorrenteResponse>> AtualizarAsync(Guid id, DespesaRecorrenteRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}
