using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IDespesaMensalAppService
{
    Task<Result<IEnumerable<DespesaMensalResponse>>> ListarPorMesAsync(Guid idUsuario, int mes, int ano);
    Task<Result<DespesaMensalResponse>> PagarAsync(Guid id, MensalStatusRequest request);
    Task<Result<DespesaMensalResponse>> EstornarAsync(Guid id);
}
