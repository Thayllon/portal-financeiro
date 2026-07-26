using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IReceitaMensalAppService
{
    Task<Result<IEnumerable<ReceitaMensalResponse>>> ListarPorMesAsync(Guid idUsuario, int mes, int ano);
    Task<Result<ReceitaMensalResponse>> ReceberAsync(Guid id, MensalStatusRequest request);
    Task<Result<ReceitaMensalResponse>> EstornarAsync(Guid id);
}
