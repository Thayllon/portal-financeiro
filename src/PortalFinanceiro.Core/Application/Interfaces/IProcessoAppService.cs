using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IProcessoAppService
{
    Task<Result<IEnumerable<ProcessoResponse>>> ListarAsync(Guid idUsuario, bool? ativo = null);
    Task<Result<ProcessoResponse>> ObterPorIdAsync(Guid id, Guid idUsuario);
    Task<Result<ProcessoResponse>> AdicionarAsync(Guid idUsuario, ProcessoRequest request);
    Task<Result<ProcessoResponse>> AtualizarAsync(Guid id, Guid idUsuario, ProcessoRequest request);
    Task<Result<Unit>> EncerrarAsync(Guid id, Guid idUsuario);
    Task<Result<Unit>> ReativarAsync(Guid id, Guid idUsuario);
    Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario);
    Task<Result<ProcessoEtapaResponse>> AdicionarEtapaAsync(Guid id, Guid idUsuario, ProcessoEtapaRequest request);
    Task<Result<ProcessoEtapaResponse>> AtualizarEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, ProcessoEtapaRequest request);
    Task<Result<ProcessoEtapaResponse>> ConcluirEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, bool forcar = false);
    Task<Result<ProcessoEtapaResponse>> EstornarEtapaAsync(Guid id, Guid etapaId, Guid idUsuario);
    Task<Result<Unit>> MoverEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, int direcao);
    Task<Result<Unit>> ExcluirEtapaAsync(Guid id, Guid etapaId, Guid idUsuario);
    Task<Result<ProcessoEtapaItemResponse>> AdicionarItemAsync(Guid id, Guid etapaId, Guid idUsuario, ProcessoEtapaItemRequest request);
    Task<Result<ProcessoEtapaItemResponse>> AtualizarItemAsync(Guid id, Guid itemId, Guid idUsuario, ProcessoEtapaItemRequest request);
    Task<Result<ProcessoEtapaItemResponse>> ConcluirItemAsync(Guid id, Guid itemId, Guid idUsuario);
    Task<Result<ProcessoEtapaItemResponse>> EstornarItemAsync(Guid id, Guid itemId, Guid idUsuario);
    Task<Result<Unit>> MoverItemAsync(Guid id, Guid itemId, Guid idUsuario, int direcao);
    Task<Result<Unit>> ExcluirItemAsync(Guid id, Guid itemId, Guid idUsuario);
}
