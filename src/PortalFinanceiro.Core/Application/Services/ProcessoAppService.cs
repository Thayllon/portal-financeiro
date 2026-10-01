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

public class ProcessoAppService : IProcessoAppService
{
    private readonly IProcessoRepository _repository;
    private readonly IParceriaRepository _parceriaRepository;
    private readonly IContratoRepository _contratoRepository;
    private readonly IModeloProcessoRepository _modeloRepository;
    private readonly IPessoaRepository _pessoaRepository;
    private readonly ILogger<ProcessoAppService> _logger;

    public ProcessoAppService(
        IProcessoRepository repository,
        IParceriaRepository parceriaRepository,
        IContratoRepository contratoRepository,
        IModeloProcessoRepository modeloRepository,
        IPessoaRepository pessoaRepository,
        ILogger<ProcessoAppService>? logger = null)
    {
        _repository = repository;
        _parceriaRepository = parceriaRepository;
        _contratoRepository = contratoRepository;
        _modeloRepository = modeloRepository;
        _pessoaRepository = pessoaRepository;
        _logger = logger ?? NullLogger<ProcessoAppService>.Instance;
    }

    public async Task<Result<IEnumerable<ProcessoResponse>>> ListarAsync(Guid idUsuario, bool? ativo = null)
    {
        var processos = await _repository.ListarAsync(idUsuario, ativo);
        var responses = new List<ProcessoResponse>();
        foreach (var p in processos)
            responses.Add(Mapear(p, null));
        return responses;
    }

    public async Task<Result<ProcessoResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var processo = await _repository.ObterProjecaoPorIdAsync(id);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");
        var etapas = await ListarEtapasComItensAsync(id);
        return Mapear(processo, etapas);
    }

    public async Task<Result<ProcessoResponse>> AdicionarAsync(Guid idUsuario, ProcessoRequest request)
    {
        var vinculo = Processo.ValidarVinculo(request.IdParceria, request.IdContrato);
        if (!vinculo.EhSucesso)
            return vinculo.Erro!;
        var validacao = await ValidarVinculoAsync(idUsuario, request.IdParceria, request.IdContrato);
        if (!validacao.EhSucesso)
            return validacao.Erro!;
        var idCliente = request.IdCliente.HasValue && request.IdCliente.Value != Guid.Empty ? request.IdCliente : null;
        var validacaoCliente = await ValidarClienteAsync(idUsuario, idCliente);
        if (!validacaoCliente.EhSucesso)
            return validacaoCliente.Erro!;

        ModeloProcesso? modelo = null;
        if (request.IdModeloProcesso.HasValue && request.IdModeloProcesso.Value != Guid.Empty)
        {
            modelo = await _modeloRepository.ObterPorIdAsync(request.IdModeloProcesso.Value);
            if (modelo is null || modelo.IdUsuario != idUsuario)
                return Erro.Validacao("MODELO_INVALIDO", "Modelo de processo não encontrado.");
        }

        var result = Processo.Criar(idUsuario, request.Nome, request.Descricao, request.IdParceria, request.IdContrato, modelo?.Id, idCliente);
        if (!result.EhSucesso)
            return result.Erro!;

        result.Dado!.DefinirCriador(idUsuario);
        await _repository.InserirAsync(result.Dado!);
        _logger.Auditar(idUsuario, "Criar", "Processo", result.Dado!.Id);

        if (modelo is not null)
            await InstanciarModeloAsync(idUsuario, result.Dado.Id, modelo.Id);

        var projecao = await _repository.ObterProjecaoPorIdAsync(result.Dado!.Id);
        var etapas = await ListarEtapasComItensAsync(result.Dado.Id);
        return Mapear(projecao!, etapas);
    }

    public async Task<Result<ProcessoResponse>> AtualizarAsync(Guid id, Guid idUsuario, ProcessoRequest request)
    {
        var processo = await _repository.ObterPorIdAsync(id);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");

        var result = processo.Atualizar(request.Nome, request.Descricao);
        if (!result.EhSucesso)
            return result.Erro!;

        processo.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(processo);
        _logger.Auditar(idUsuario, "Atualizar", "Processo", processo.Id);
        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        var etapas = await ListarEtapasComItensAsync(id);
        return Mapear(projecao!, etapas);
    }

    public async Task<Result<Unit>> EncerrarAsync(Guid id, Guid idUsuario)
    {
        var processo = await _repository.ObterPorIdAsync(id);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");
        if (!processo.Ativo)
            return Erro.Negocio("PROCESSO_JA_ENCERRADO", "Este processo já está encerrado.");

        var pendentes = await _repository.ContarEtapasPendentesAsync(id);
        if (pendentes > 0)
            return Erro.Negocio("PROCESSO_COM_ETAPAS_PENDENTES", "Só é possível encerrar processo com todas as etapas concluídas.");

        processo.Desativar();
        processo.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(processo);
        _logger.Auditar(idUsuario, "Encerrar", "Processo", processo.Id);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ReativarAsync(Guid id, Guid idUsuario)
    {
        var processo = await _repository.ObterPorIdAsync(id);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");
        if (processo.Ativo)
            return Erro.Negocio("PROCESSO_JA_ATIVO", "Este processo já está ativo.");

        processo.Reativar();
        processo.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(processo);
        _logger.Auditar(idUsuario, "Reativar", "Processo", processo.Id);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var processo = await _repository.ObterPorIdAsync(id);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");

        var etapas = await _repository.ListarEtapasAsync(id);
        foreach (var etapa in etapas)
        {
            var itens = await _repository.ListarItensAsync(etapa.Id);
            foreach (var item in itens)
                await _repository.ExcluirItemAsync(item.Id);
            await _repository.ExcluirEtapaAsync(etapa.Id);
        }
        await _repository.ExcluirProcessoAsync(id);
        _logger.Auditar(idUsuario, "Excluir", "Processo", id);
        return Resultado.Sucesso();
    }

    public async Task<Result<ProcessoEtapaResponse>> AdicionarEtapaAsync(Guid id, Guid idUsuario, ProcessoEtapaRequest request)
    {
        var processo = await _repository.ObterPorIdAsync(id);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");

        var ordem = await _repository.ProximaOrdemAsync(id);
        var result = ProcessoEtapa.Criar(id, request.Nome, request.Descricao, ordem, request.DataPrevista?.Date);
        if (!result.EhSucesso)
            return result.Erro!;

        result.Dado!.DefinirCriador(idUsuario);
        await _repository.InserirEtapaAsync(result.Dado!);
        _logger.Auditar(idUsuario, "Criar", "ProcessoEtapa", result.Dado!.Id);
        return await MapearEtapaAsync(result.Dado!);
    }

    public async Task<Result<ProcessoEtapaResponse>> AtualizarEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, ProcessoEtapaRequest request)
    {
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        var result = etapa.Dado!.Atualizar(request.Nome, request.Descricao, request.DataPrevista?.Date);
        if (!result.EhSucesso)
            return result.Erro!;

        etapa.Dado!.DefinirEditor(idUsuario);
        await _repository.AtualizarEtapaAsync(etapa.Dado!);
        return await MapearEtapaAsync(etapa.Dado!);
    }

    public async Task<Result<ProcessoEtapaResponse>> ConcluirEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, bool forcar = false)
    {
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        if (!forcar)
        {
            var pendencia = await ValidarItensObrigatoriosAsync(etapaId);
            if (!pendencia.EhSucesso)
                return pendencia.Erro!;
        }

        etapa.Dado!.MarcarConcluida();
        etapa.Dado!.DefinirEditor(idUsuario);
        await _repository.AtualizarEtapaAsync(etapa.Dado!);

        var processo = await _repository.ObterPorIdAsync(id);
        var etapas = (await _repository.ListarEtapasAsync(id)).OrderBy(e => e.Ordem).ToList();
        var proxima = etapas.SkipWhile(e => e.Id != etapaId).Skip(1).FirstOrDefault();
        if (proxima is not null && !proxima.Concluida)
        {
            proxima.Iniciar();
            proxima.DefinirEditor(idUsuario);
            await _repository.AtualizarEtapaAsync(proxima);
        }

        if (processo is not null && processo.Ativo && etapas.All(e => e.Id == etapaId || e.Concluida))
        {
            processo.Desativar();
            processo.DefinirEditor(idUsuario);
            await _repository.AtualizarAsync(processo);
        }

        return await MapearEtapaAsync(etapa.Dado!);
    }

    public async Task<Result<ProcessoEtapaResponse>> EstornarEtapaAsync(Guid id, Guid etapaId, Guid idUsuario)
    {
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        etapa.Dado!.Estornar();
        etapa.Dado!.DefinirEditor(idUsuario);
        await _repository.AtualizarEtapaAsync(etapa.Dado!);

        var processo = await _repository.ObterPorIdAsync(id);
        if (processo is not null && !processo.Ativo)
        {
            processo.Reativar();
            processo.DefinirEditor(idUsuario);
            await _repository.AtualizarAsync(processo);
        }

        return await MapearEtapaAsync(etapa.Dado!);
    }

    public async Task<Result<Unit>> MoverEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, int direcao)
    {
        if (direcao != -1 && direcao != 1)
            return Erro.Validacao("DIRECAO_INVALIDA", "Direção deve ser -1 (subir) ou 1 (descer).");

        var processo = await _repository.ObterPorIdAsync(id);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");

        var etapas = (await _repository.ListarEtapasAsync(id)).OrderBy(e => e.Ordem).ToList();
        var indice = etapas.FindIndex(e => e.Id == etapaId);
        if (indice < 0)
            return Erro.NaoEncontrado("Etapa");
        var destino = indice + direcao;
        if (destino < 0 || destino >= etapas.Count)
            return Erro.Negocio("ETAPA_LIMITE_ORDEM", "A etapa já está no limite da ordem.");

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
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        var itens = await _repository.ListarItensAsync(etapaId);
        foreach (var item in itens)
            await _repository.ExcluirItemAsync(item.Id);
        await _repository.ExcluirEtapaAsync(etapaId);
        return Resultado.Sucesso();
    }

    public async Task<Result<ProcessoEtapaItemResponse>> AdicionarItemAsync(Guid id, Guid etapaId, Guid idUsuario, ProcessoEtapaItemRequest request)
    {
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        var ordem = await _repository.ProximaOrdemItemAsync(etapaId);
        var result = ProcessoEtapaItem.Criar(etapaId, request.Nome, request.Descricao, request.Obrigatorio, request.ExigeAnexo, ordem);
        if (!result.EhSucesso)
            return result.Erro!;

        result.Dado!.DefinirCriador(idUsuario);
        await _repository.InserirItemAsync(result.Dado!);
        _logger.Auditar(idUsuario, "Criar", "ProcessoEtapaItem", result.Dado!.Id);
        return await MapearItemAsync(result.Dado!);
    }

    public async Task<Result<ProcessoEtapaItemResponse>> AtualizarItemAsync(Guid id, Guid itemId, Guid idUsuario, ProcessoEtapaItemRequest request)
    {
        var item = await ObterItemDoProcessoAsync(id, itemId, idUsuario);
        if (!item.EhSucesso)
            return item.Erro!;

        var result = item.Dado!.Atualizar(request.Nome, request.Descricao, request.Obrigatorio, request.ExigeAnexo);
        if (!result.EhSucesso)
            return result.Erro!;

        item.Dado!.DefinirEditor(idUsuario);
        await _repository.AtualizarItemAsync(item.Dado!);
        return await MapearItemAsync(item.Dado!);
    }

    public async Task<Result<ProcessoEtapaItemResponse>> ConcluirItemAsync(Guid id, Guid itemId, Guid idUsuario)
    {
        var item = await ObterItemDoProcessoAsync(id, itemId, idUsuario);
        if (!item.EhSucesso)
            return item.Erro!;

        item.Dado!.MarcarConcluida();
        item.Dado!.DefinirEditor(idUsuario);
        await _repository.AtualizarItemAsync(item.Dado!);

        var itens = (await _repository.ListarItensAsync(item.Dado.IdProcessoEtapa)).OrderBy(i => i.Ordem).ToList();
        var proximo = itens.SkipWhile(i => i.Id != itemId).Skip(1).FirstOrDefault();
        if (proximo is not null && !proximo.Concluida)
        {
            proximo.Iniciar();
            proximo.DefinirEditor(idUsuario);
            await _repository.AtualizarItemAsync(proximo);
        }

        return await MapearItemAsync(item.Dado!);
    }

    public async Task<Result<ProcessoEtapaItemResponse>> EstornarItemAsync(Guid id, Guid itemId, Guid idUsuario)
    {
        var item = await ObterItemDoProcessoAsync(id, itemId, idUsuario);
        if (!item.EhSucesso)
            return item.Erro!;

        item.Dado!.Estornar();
        item.Dado!.DefinirEditor(idUsuario);
        await _repository.AtualizarItemAsync(item.Dado!);
        return await MapearItemAsync(item.Dado!);
    }

    public async Task<Result<Unit>> MoverItemAsync(Guid id, Guid itemId, Guid idUsuario, int direcao)
    {
        if (direcao != -1 && direcao != 1)
            return Erro.Validacao("DIRECAO_INVALIDA", "Direção deve ser -1 (subir) ou 1 (descer).");

        var item = await ObterItemDoProcessoAsync(id, itemId, idUsuario);
        if (!item.EhSucesso)
            return item.Erro!;

        var itens = (await _repository.ListarItensAsync(item.Dado!.IdProcessoEtapa)).OrderBy(i => i.Ordem).ToList();
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
        var item = await ObterItemDoProcessoAsync(id, itemId, idUsuario);
        if (!item.EhSucesso)
            return item.Erro!;

        await _repository.ExcluirItemAsync(itemId);
        return Resultado.Sucesso();
    }

    private async Task<Result<ProcessoEtapa>> ObterEtapaDoProcessoAsync(Guid idProcesso, Guid etapaId, Guid idUsuario)
    {
        var processo = await _repository.ObterPorIdAsync(idProcesso);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");

        var etapa = await _repository.ObterEtapaPorIdAsync(etapaId);
        if (etapa is null || etapa.IdProcesso != idProcesso)
            return Erro.NaoEncontrado("Etapa");
        return etapa;
    }

    private async Task<Result<ProcessoEtapaItem>> ObterItemDoProcessoAsync(Guid idProcesso, Guid itemId, Guid idUsuario)
    {
        var processo = await _repository.ObterPorIdAsync(idProcesso);
        if (processo is null)
            return Erro.NaoEncontrado("Processo");
        if (processo.IdUsuario != idUsuario)
            return Erro.Permissao("PROCESSO_ACESSO_NEGADO", "Processo de outro usuário.");

        var item = await _repository.ObterItemPorIdAsync(itemId);
        if (item is null)
            return Erro.NaoEncontrado("Item");
        var etapa = await _repository.ObterEtapaPorIdAsync(item.IdProcessoEtapa);
        if (etapa is null || etapa.IdProcesso != idProcesso)
            return Erro.NaoEncontrado("Item");
        return item;
    }

    private async Task<Result<Unit>> ValidarItensObrigatoriosAsync(Guid etapaId)
    {
        var pendentes = await _repository.ContarItensObrigatoriosPendentesAsync(etapaId);
        if (pendentes == 0)
            return Resultado.Sucesso();

        var itens = await _repository.ListarItensAsync(etapaId);
        foreach (var item in itens.Where(i => i.Obrigatorio && !i.Concluida))
        {
            if (item.ExigeAnexo)
            {
                var anexos = await _repository.ContarAnexosPorItemAsync(item.Id);
                if (anexos == 0)
                    return Erro.Negocio("ETAPA_ITEM_SEM_ANEXO", $"O item \"{item.Nome}\" exige ao menos um anexo.");
            }
        }
        return Erro.Negocio("ETAPA_COM_ITENS_PENDENTES", "Conclua os itens obrigatórios da fase antes de concluí-la.");
    }

    private async Task<Result<Unit>> ValidarClienteAsync(Guid idUsuario, Guid? idCliente)
    {
        if (!idCliente.HasValue || idCliente.Value == Guid.Empty)
            return Resultado.Sucesso();

        var cliente = await _pessoaRepository.ObterPorIdAsync(idCliente.Value);
        if (cliente is null || cliente.IdUsuario != idUsuario)
            return Erro.Validacao("CLIENTE_INVALIDO", "Cliente não encontrado.");
        if (!cliente.Ativo)
            return Erro.Validacao("CLIENTE_INATIVO", "Cliente está inativo.");
        return Resultado.Sucesso();
    }

    private async Task InstanciarModeloAsync(Guid idUsuario, Guid idProcesso, Guid idModelo)
    {
        var etapas = (await _modeloRepository.ListarEtapasAsync(idModelo)).OrderBy(e => e.Ordem).ToList();
        ProcessoEtapa? primeiraEtapa = null;
        foreach (var etapaModelo in etapas)
        {
            var etapa = ProcessoEtapa.Criar(idProcesso, etapaModelo.Nome, etapaModelo.Descricao, etapaModelo.Ordem, null);
            if (!etapa.EhSucesso)
                continue;
            etapa.Dado!.DefinirCriador(idUsuario);
            await _repository.InserirEtapaAsync(etapa.Dado!);
            primeiraEtapa ??= etapa.Dado!;

            var itensModelo = (await _modeloRepository.ListarItensAsync(etapaModelo.Id)).OrderBy(i => i.Ordem).ToList();
            ProcessoEtapaItem? primeiroItem = null;
            foreach (var itemModelo in itensModelo)
            {
                var item = ProcessoEtapaItem.Criar(etapa.Dado.Id, itemModelo.Nome, itemModelo.Descricao, itemModelo.Obrigatorio, itemModelo.ExigeAnexo, itemModelo.Ordem);
                if (!item.EhSucesso)
                    continue;
                item.Dado!.DefinirCriador(idUsuario);
                await _repository.InserirItemAsync(item.Dado!);
                primeiroItem ??= item.Dado!;
            }
            primeiroItem?.Iniciar();
            if (primeiroItem is not null)
                await _repository.AtualizarItemAsync(primeiroItem);
        }
        if (primeiraEtapa is not null)
        {
            primeiraEtapa.Iniciar();
            await _repository.AtualizarEtapaAsync(primeiraEtapa);
        }
    }

    private async Task<List<(ProcessoEtapa Etapa, List<ProcessoEtapaItem> Itens)> > ListarEtapasComItensAsync(Guid idProcesso)
    {
        var etapas = (await _repository.ListarEtapasAsync(idProcesso)).OrderBy(e => e.Ordem).ToList();
        var resultado = new List<(ProcessoEtapa, List<ProcessoEtapaItem>)>();
        foreach (var etapa in etapas)
        {
            var itens = (await _repository.ListarItensAsync(etapa.Id)).OrderBy(i => i.Ordem).ToList();
            resultado.Add((etapa, itens));
        }
        return resultado;
    }

    private async Task<Result<Unit>> ValidarVinculoAsync(Guid idUsuario, Guid? idParceria, Guid? idContrato)
    {
        if (idParceria.HasValue && idParceria.Value != Guid.Empty)
        {
            var parceria = await _parceriaRepository.ObterPorIdAsync(idParceria.Value);
            if (parceria is null || parceria.IdUsuario != idUsuario)
                return Erro.Validacao("PARCERIA_INVALIDA", "Parceria não encontrada.");
            if (!parceria.Ativo)
                return Erro.Validacao("PARCERIA_INATIVA", "Parceria está encerrada.");
        }

        if (idContrato.HasValue && idContrato.Value != Guid.Empty)
        {
            var contrato = await _contratoRepository.ObterPorIdAsync(idContrato.Value);
            if (contrato is null || contrato.IdUsuario != idUsuario)
                return Erro.Validacao("CONTRATO_INVALIDO", "Contrato não encontrado.");
            if (!contrato.Ativo)
                return Erro.Validacao("CONTRATO_INATIVO", "Contrato está encerrado.");
        }

        return Resultado.Sucesso();
    }

    private static ProcessoResponse Mapear(ProcessoProjecao p, IEnumerable<(ProcessoEtapa Etapa, List<ProcessoEtapaItem> Itens)>? etapas)
    {
        var lista = etapas?.Select(t => MapearEtapa(t.Etapa, t.Itens)).ToList() ?? new List<ProcessoEtapaResponse>();
        var percentual = p.TotalItens > 0
            ? (int)Math.Round(p.ItensConcluidos * 100m / p.TotalItens)
            : p.TotalEtapas == 0 ? 0 : (int)Math.Round(p.EtapasConcluidas * 100m / p.TotalEtapas);
        return new ProcessoResponse
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            IdParceria = p.IdParceria,
            IdContrato = p.IdContrato,
            IdModeloProcesso = p.IdModeloProcesso,
            ModeloNome = p.ModeloNome,
            IdCliente = p.IdCliente,
            VinculoTipo = p.VinculoTipo,
            VinculoNome = p.VinculoNome,
            Cliente = p.Cliente,
            Ativo = p.Ativo,
            DataEncerramento = p.DataEncerramento,
            DataCadastro = p.DataCadastro,
            CriadoPor = p.CriadoPor,
            AlteradoPor = p.AlteradoPor,
            FaseAtual = p.FaseAtual,
            TotalEtapas = p.TotalEtapas,
            EtapasConcluidas = p.EtapasConcluidas,
            TotalItens = p.TotalItens,
            ItensConcluidos = p.ItensConcluidos,
            PercentualConcluido = percentual,
            Etapas = lista
        };
    }

    private static ProcessoEtapaResponse MapearEtapa(ProcessoEtapa e, IEnumerable<ProcessoEtapaItem> itens)
        => new()
        {
            Id = e.Id,
            Nome = e.Nome,
            Descricao = e.Descricao,
            Ordem = e.Ordem,
            Concluida = e.Concluida,
            DataPrevista = e.DataPrevista,
            DataInicio = e.DataInicio,
            DataConclusao = e.DataConclusao,
            CriadoPor = e.CriadoPor,
            AlteradoPor = e.AlteradoPor,
            Itens = itens.OrderBy(i => i.Ordem).Select(MapearItem).ToList()
        };

    private async Task<ProcessoEtapaResponse> MapearEtapaAsync(ProcessoEtapa e)
    {
        var itens = await _repository.ListarItensAsync(e.Id);
        return MapearEtapa(e, itens);
    }

    private async Task<ProcessoEtapaItemResponse> MapearItemAsync(ProcessoEtapaItem i)
    {
        var anexos = await _repository.ContarAnexosPorItemAsync(i.Id);
        return MapearItem(i, anexos);
    }

    private static ProcessoEtapaItemResponse MapearItem(ProcessoEtapaItem i, int totalAnexos = 0)
        => new()
        {
            Id = i.Id,
            Nome = i.Nome,
            Descricao = i.Descricao,
            Obrigatorio = i.Obrigatorio,
            ExigeAnexo = i.ExigeAnexo,
            Ordem = i.Ordem,
            Concluida = i.Concluida,
            DataInicio = i.DataInicio,
            DataConclusao = i.DataConclusao,
            TotalAnexos = totalAnexos,
            CriadoPor = i.CriadoPor,
            AlteradoPor = i.AlteradoPor
        };
}
