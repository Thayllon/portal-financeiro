using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IReceitaAppService
{
    Task<Result<IEnumerable<ReceitaResponse>>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null);
    Task<Result<ReceitaResponse>> ObterPorIdAsync(Guid id);
    Task<Result<ReceitaResponse>> AdicionarAsync(Guid idUsuario, ReceitaRequest request);
    Task<Result<ReceitaResponse>> AtualizarAsync(Guid id, ReceitaRequest request);
    Task<Result<ReceitaResponse>> ReceberAsync(Guid id, MensalStatusRequest request);
    Task<Result<ReceitaResponse>> EstornarAsync(Guid id);
    Task<Result<Unit>> ExcluirAsync(Guid id);
}
