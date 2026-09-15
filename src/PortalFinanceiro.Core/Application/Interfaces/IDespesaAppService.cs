using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IDespesaAppService
{
    Task<Result<IEnumerable<DespesaResponse>>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null);
    Task<Result<IEnumerable<DespesaResponse>>> ListarPorParceriaAsync(Guid idUsuario, Guid idParceria);
    Task<Result<DespesaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario);
    Task<Result<DespesaResponse>> AdicionarAsync(Guid idUsuario, DespesaRequest request);
    Task<Result<DespesaResponse>> AtualizarAsync(Guid id, Guid idUsuario, DespesaRequest request);
    Task<Result<DespesaResponse>> PagarAsync(Guid id, Guid idUsuario, MensalStatusRequest request);
    Task<Result<DespesaResponse>> EstornarAsync(Guid id, Guid idUsuario);
    Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario);
}
