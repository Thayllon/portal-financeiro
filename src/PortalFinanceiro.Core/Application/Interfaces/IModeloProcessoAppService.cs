using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IModeloProcessoAppService
{
    Task<Result<IEnumerable<ModeloProcessoResponse>>> ListarAsync(Guid idUsuario, bool? ativo = null);
    Task<Result<ModeloProcessoResponse>> ObterPorIdAsync(Guid id, Guid idUsuario);
    Task<Result<ModeloProcessoResponse>> AdicionarAsync(Guid idUsuario, ModeloProcessoRequest request);
    Task<Result<ModeloProcessoResponse>> AtualizarAsync(Guid id, Guid idUsuario, ModeloProcessoRequest request);
    Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario);
    Task<Result<ModeloProcessoResponse>> DuplicarAsync(Guid id, Guid idUsuario);
    Task<Result<ModeloEtapaResponse>> AdicionarEtapaAsync(Guid id, Guid idUsuario, ModeloEtapaRequest request);
    Task<Result<ModeloEtapaResponse>> AtualizarEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, ModeloEtapaRequest request);
    Task<Result<Unit>> MoverEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, int direcao);
    Task<Result<Unit>> ExcluirEtapaAsync(Guid id, Guid etapaId, Guid idUsuario);
    Task<Result<ModeloItemResponse>> AdicionarItemAsync(Guid id, Guid etapaId, Guid idUsuario, ModeloItemRequest request);
    Task<Result<ModeloItemResponse>> AtualizarItemAsync(Guid id, Guid itemId, Guid idUsuario, ModeloItemRequest request);
    Task<Result<Unit>> MoverItemAsync(Guid id, Guid itemId, Guid idUsuario, int direcao);
    Task<Result<Unit>> ExcluirItemAsync(Guid id, Guid itemId, Guid idUsuario);
}
