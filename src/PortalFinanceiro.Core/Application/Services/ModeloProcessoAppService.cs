using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Core.Domain.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PortalFinanceiro.Core.Application.Services;

public class ModeloProcessoAppService : IModeloProcessoAppService
{
    private readonly IModeloProcessoRepository _repository;
    private readonly ILogger<ModeloProcessoAppService> _logger;

    public ModeloProcessoAppService(
        IModeloProcessoRepository repository,
        ILogger<ModeloProcessoAppService>? logger = null)
    {
        _repository = repository;
        _logger = logger ?? NullLogger<ModeloProcessoAppService>.Instance;
    }

    public async Task<Result<IEnumerable<ModeloProcessoResponse>>> ListarAsync(Guid idUsuario, bool? ativo = null)
    {
        var modelos = await _repository.ListarAsync(idUsuario, ativo);
        var responses = new List<ModeloProcessoResponse>();
        foreach (var m in modelos)
            responses.Add(await MapearAsync(m));
        return responses;
    }

    public async Task<Result<ModeloProcessoResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var modelo = await _repository.ObterProjecaoPorIdAsync(id);
        if (modelo is null)
            return Erro.NaoEncontrado("Modelo de processo");
        if (modelo.IdUsuario != idUsuario)
            return Erro.Permissao("MODELO_ACESSO_NEGADO", "Modelo de processo de outro usuário.");
        return await MapearAsync(modelo);
    }

    public async Task<Result<ModeloProcessoResponse>> AdicionarAsync(Guid idUsuario, ModeloProcessoRequest request)
    {
        var result = ModeloProcesso.Criar(idUsuario, request.Nome, request.Descricao);
        if (!result.EhSucesso)
            return result.Erro!;

        result.Dado!.DefinirCriador(idUsuario);
        await _repository.InserirAsync(result.Dado!);
        _logger.Auditar(idUsuario, "Criar", "ModeloProcesso", result.Dado!.Id);
        var projecao = await _repository.ObterProjecaoPorIdAsync(result.Dado!.Id);
        return await MapearAsync(projecao!);
    }

    public async Task<Result<ModeloProcessoResponse>> AtualizarAsync(Guid id, Guid idUsuario, ModeloProcessoRequest request)
    {
        var modelo = await _repository.ObterPorIdAsync(id);
        if (modelo is null)
            return Erro.NaoEncontrado("Modelo de processo");
        if (modelo.IdUsuario != idUsuario)
            return Erro.Permissao("MODELO_ACESSO_NEGADO", "Modelo de processo de outro usuário.");

        var result = modelo.Atualizar(request.Nome, request.Descricao);
        if (!result.EhSucesso)
            return result.Erro!;

        modelo.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(modelo);
        _logger.Auditar(idUsuario, "Atualizar", "ModeloProcesso", modelo.Id);
        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return await MapearAsync(projecao!);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var modelo = await _repository.ObterPorIdAsync(id);
        if (modelo is null)
            return Erro.NaoEncontrado("Modelo de processo");
        if (modelo.IdUsuario != idUsuario)
            return Erro.Permissao("MODELO_ACESSO_NEGADO", "Modelo de processo de outro usuário.");

        var etapas = await _repository.ListarEtapasAsync(id);
        foreach (var etapa in etapas)
        {
            var itens = await _repository.ListarItensAsync(etapa.Id);
            foreach (var item in itens)
                await _repository.ExcluirItemAsync(item.Id);
            await _repository.ExcluirEtapaAsync(etapa.Id);
        }
        await _repository.ExcluirAsync(id);
        _logger.Auditar(idUsuario, "Excluir", "ModeloProcesso", id);
        return Resultado.Sucesso();
    }

    public async Task<Result<ModeloProcessoResponse>> DuplicarAsync(Guid id, Guid idUsuario)
    {
        var origem = await ObterPorIdAsync(id, idUsuario);
        if (!origem.EhSucesso)
            return origem.Erro!;

        var copia = ModeloProcesso.Criar(idUsuario, origem.Dado!.Nome + " (cópia)", origem.Dado.Descricao);
        if (!copia.EhSucesso)
            return copia.Erro!;
        copia.Dado!.DefinirCriador(idUsuario);
        await _repository.InserirAsync(copia.Dado!);

        foreach (var etapa in origem.Dado.Etapas)
        {
            var novaEtapa = ModeloEtapa.Criar(copia.Dado.Id, etapa.Nome, etapa.Descricao, etapa.Ordem);
            if (!novaEtapa.EhSucesso)
                return novaEtapa.Erro!;
            novaEtapa.Dado!.DefinirCriador(idUsuario);
            await _repository.InserirEtapaAsync(novaEtapa.Dado!);

            foreach (var item in etapa.Itens)
            {
                var novoItem = ModeloItem.Criar(novaEtapa.Dado.Id, item.Nome, item.Descricao, item.Obrigatorio, item.ExigeAnexo, item.Ordem);
                if (!novoItem.EhSucesso)
                    return novoItem.Erro!;
                novoItem.Dado!.DefinirCriador(idUsuario);
                await _repository.InserirItemAsync(novoItem.Dado!);
            }
        }

        _logger.Auditar(idUsuario, "Duplicar", "ModeloProcesso", copia.Dado.Id);
        var projecao = await _repository.ObterProjecaoPorIdAsync(copia.Dado.Id);
        return await MapearAsync(projecao!);
    }

    public async Task<Result<ModeloEtapaResponse>> AdicionarEtapaAsync(Guid id, Guid idUsuario, ModeloEtapaRequest request)
    {
        var modelo = await _repository.ObterPorIdAsync(id);
        if (modelo is null)
            return Erro.NaoEncontrado("Modelo de processo");
        if (modelo.IdUsuario != idUsuario)
            return Erro.Permissao("MODELO_ACESSO_NEGADO", "Modelo de processo de outro usuário.");

        var ordem = await _repository.ProximaOrdemEtapaAsync(id);
        var result = ModeloEtapa.Criar(id, request.Nome, request.Descricao, ordem);
        if (!result.EhSucesso)
            return result.Erro!;

        result.Dado!.DefinirCriador(idUsuario);
        await _repository.InserirEtapaAsync(result.Dado!);
        _logger.Auditar(idUsuario, "Criar", "ModeloEtapa", result.Dado!.Id);
        return MapearEtapa(result.Dado!, new List<ModeloItem>());
    }

    public async Task<Result<ModeloEtapaResponse>> AtualizarEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, ModeloEtapaRequest request)
    {
        var etapa = await ObterEtapaDoModeloAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        var result = etapa.Dado!.Atualizar(request.Nome, request.Descricao);
        if (!result.EhSucesso)
            return result.Erro!;

        etapa.Dado!.DefinirEditor(idUsuario);
        await _repository.AtualizarEtapaAsync(etapa.Dado!);
        var itens = await _repository.ListarItensAsync(etapaId);
        return MapearEtapa(etapa.Dado!, itens);
    }

    public async Task<Result<Unit>> MoverEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, int direcao)
    {
        if (direcao != -1 && direcao != 1)
            return Erro.Validacao("DIRECAO_INVALIDA", "Direção deve ser -1 (subir) ou 1 (descer).");

        var modelo = await _repository.ObterPorIdAsync(id);
        if (modelo is null)
            return Erro.NaoEncontrado("Modelo de processo");
        if (modelo.IdUsuario != idUsuario)
            return Erro.Permissao("MODELO_ACESSO_NEGADO", "Modelo de processo de outro usuário.");

        var etapas = (await _repository.ListarEtapasAsync(id)).OrderBy(e => e.Ordem).ToList();
        var indice = etapas.FindIndex(e => e.Id == etapaId);
        if (indice < 0)
            return Erro.NaoEncontrado("Fase");
        var destino = indice + direcao;
        if (destino < 0 || destino >= etapas.Count)
            return Erro.Negocio("ETAPA_LIMITE_ORDEM", "A fase já está no limite da ordem.");

        var atual = etapas[indice];
        var vizinha = etapas[destino];
        var ordemAtual = atual.Ordem;
        atual.MoverPara(vizinha.Ordem);
        vizinha.MoverPara(ordemAtual);
        atual.DefinirEditor(idUsuario);
        vizinha.DefinirEditor(idUsuario);
        await _repository.AtualizarEtapaAsync(atual);
        await _repository.AtualizarEtapaAsync(vizinha);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ExcluirEtapaAsync(Guid id, Guid etapaId, Guid idUsuario)
    {
        var etapa = await ObterEtapaDoModeloAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        var itens = await _repository.ListarItensAsync(etapaId);
        foreach (var item in itens)
            await _repository.ExcluirItemAsync(item.Id);
        await _repository.ExcluirEtapaAsync(etapaId);
        return Resultado.Sucesso();
    }

    public async Task<Result<ModeloItemResponse>> AdicionarItemAsync(Guid id, Guid etapaId, Guid idUsuario, ModeloItemRequest request)
    {
        var etapa = await ObterEtapaDoModeloAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        var ordem = await _repository.ProximaOrdemItemAsync(etapaId);
        var result = ModeloItem.Criar(etapaId, request.Nome, request.Descricao, request.Obrigatorio, request.ExigeAnexo, ordem);
        if (!result.EhSucesso)
            return result.Erro!;

        result.Dado!.DefinirCriador(idUsuario);
        await _repository.InserirItemAsync(result.Dado!);
        _logger.Auditar(idUsuario, "Criar", "ModeloItem", result.Dado!.Id);
        return MapearItem(result.Dado!);
    }

    public async Task<Result<ModeloItemResponse>> AtualizarItemAsync(Guid id, Guid itemId, Guid idUsuario, ModeloItemRequest request)
    {
        var item = await ObterItemDoModeloAsync(id, itemId, idUsuario);
        if (!item.EhSucesso)
            return item.Erro!;

        var result = item.Dado!.Atualizar(request.Nome, request.Descricao, request.Obrigatorio, request.ExigeAnexo);
        if (!result.EhSucesso)
            return result.Erro!;

        item.Dado!.DefinirEditor(idUsuario);
        await _repository.AtualizarItemAsync(item.Dado!);
        return MapearItem(item.Dado!);
    }

    public async Task<Result<Unit>> MoverItemAsync(Guid id, Guid itemId, Guid idUsuario, int direcao)
    {
        if (direcao != -1 && direcao != 1)
            return Erro.Validacao("DIRECAO_INVALIDA", "Direção deve ser -1 (subir) ou 1 (descer).");

        var item = await ObterItemDoModeloAsync(id, itemId, idUsuario);
        if (!item.EhSucesso)
            return item.Erro!;

        var itens = (await _repository.ListarItensAsync(item.Dado!.IdModeloEtapa)).OrderBy(i => i.Ordem).ToList();
        var indice = itens.FindIndex(i => i.Id == itemId);
        var destino = indice + direcao;
        if (destino < 0 || destino >= itens.Count)
            return Erro.Negocio("ITEM_LIMITE_ORDEM", "O item já está no limite da ordem.");

        var atual = itens[indice];
        var vizinho = itens[destino];
        var ordemAtual = atual.Ordem;
        atual.MoverPara(vizinho.Ordem);
        vizinho.MoverPara(ordemAtual);
        atual.DefinirEditor(idUsuario);
        vizinho.DefinirEditor(idUsuario);
        await _repository.AtualizarItemAsync(atual);
        await _repository.AtualizarItemAsync(vizinho);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ExcluirItemAsync(Guid id, Guid itemId, Guid idUsuario)
    {
        var item = await ObterItemDoModeloAsync(id, itemId, idUsuario);
        if (!item.EhSucesso)
            return item.Erro!;

        await _repository.ExcluirItemAsync(itemId);
        return Resultado.Sucesso();
    }

    private async Task<Result<ModeloEtapa>> ObterEtapaDoModeloAsync(Guid idModelo, Guid etapaId, Guid idUsuario)
    {
        var modelo = await _repository.ObterPorIdAsync(idModelo);
        if (modelo is null)
            return Erro.NaoEncontrado("Modelo de processo");
        if (modelo.IdUsuario != idUsuario)
            return Erro.Permissao("MODELO_ACESSO_NEGADO", "Modelo de processo de outro usuário.");

        var etapa = await _repository.ObterEtapaPorIdAsync(etapaId);
        if (etapa is null || etapa.IdModeloProcesso != idModelo)
            return Erro.NaoEncontrado("Fase");
        return etapa;
    }

    private async Task<Result<ModeloItem>> ObterItemDoModeloAsync(Guid idModelo, Guid itemId, Guid idUsuario)
    {
        var item = await _repository.ObterItemPorIdAsync(itemId);
        if (item is null)
            return Erro.NaoEncontrado("Item");
        var etapa = await _repository.ObterEtapaPorIdAsync(item.IdModeloEtapa);
        if (etapa is null || etapa.IdModeloProcesso != idModelo)
            return Erro.NaoEncontrado("Item");
        var modelo = await _repository.ObterPorIdAsync(idModelo);
        if (modelo is null)
            return Erro.NaoEncontrado("Modelo de processo");
        if (modelo.IdUsuario != idUsuario)
            return Erro.Permissao("MODELO_ACESSO_NEGADO", "Modelo de processo de outro usuário.");
        return item;
    }

    private async Task<ModeloProcessoResponse> MapearAsync(ModeloProcessoProjecao m)
    {
        var etapas = (await _repository.ListarEtapasAsync(m.Id)).OrderBy(e => e.Ordem).ToList();
        var lista = new List<ModeloEtapaResponse>();
        foreach (var etapa in etapas)
        {
            var itens = await _repository.ListarItensAsync(etapa.Id);
            lista.Add(MapearEtapa(etapa, itens));
        }
        return new ModeloProcessoResponse
        {
            Id = m.Id,
            Nome = m.Nome,
            Descricao = m.Descricao,
            Ativo = m.Ativo,
            DataCadastro = m.DataCadastro,
            CriadoPor = m.CriadoPor,
            AlteradoPor = m.AlteradoPor,
            TotalEtapas = m.TotalEtapas,
            TotalItens = m.TotalItens,
            Etapas = lista
        };
    }

    private static ModeloEtapaResponse MapearEtapa(ModeloEtapa e, IEnumerable<ModeloItem> itens)
        => new()
        {
            Id = e.Id,
            Nome = e.Nome,
            Descricao = e.Descricao,
            Ordem = e.Ordem,
            CriadoPor = e.CriadoPor,
            AlteradoPor = e.AlteradoPor,
            Itens = itens.OrderBy(i => i.Ordem).Select(MapearItem).ToList()
        };

    private static ModeloItemResponse MapearItem(ModeloItem i)
        => new()
        {
            Id = i.Id,
            Nome = i.Nome,
            Descricao = i.Descricao,
            Obrigatorio = i.Obrigatorio,
            ExigeAnexo = i.ExigeAnexo,
            Ordem = i.Ordem,
            CriadoPor = i.CriadoPor,
            AlteradoPor = i.AlteradoPor
        };
}
