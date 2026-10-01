using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class ProcessoAppService : IProcessoAppService
{
    private readonly IProcessoRepository _repository;
    private readonly IParceriaRepository _parceriaRepository;
    private readonly IContratoRepository _contratoRepository;

    public ProcessoAppService(
        IProcessoRepository repository,
        IParceriaRepository parceriaRepository,
        IContratoRepository contratoRepository)
    {
        _repository = repository;
        _parceriaRepository = parceriaRepository;
        _contratoRepository = contratoRepository;
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
        var etapas = await _repository.ListarEtapasAsync(id);
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

        var result = Processo.Criar(idUsuario, request.Nome, request.Descricao, request.IdParceria, request.IdContrato);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.InserirAsync(result.Dado!);
        var projecao = await _repository.ObterProjecaoPorIdAsync(result.Dado!.Id);
        return Mapear(projecao!, null);
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

        await _repository.AtualizarAsync(processo);
        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        var etapas = await _repository.ListarEtapasAsync(id);
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
        await _repository.AtualizarAsync(processo);
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
        await _repository.AtualizarAsync(processo);
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
            await _repository.ExcluirEtapaAsync(etapa.Id);
        await _repository.ExcluirProcessoAsync(id);
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

        await _repository.InserirEtapaAsync(result.Dado!);
        return MapearEtapa(result.Dado!);
    }

    public async Task<Result<ProcessoEtapaResponse>> AtualizarEtapaAsync(Guid id, Guid etapaId, Guid idUsuario, ProcessoEtapaRequest request)
    {
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        var result = etapa.Dado!.Atualizar(request.Nome, request.Descricao, request.DataPrevista?.Date);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarEtapaAsync(etapa.Dado!);
        return MapearEtapa(etapa.Dado!);
    }

    public async Task<Result<ProcessoEtapaResponse>> ConcluirEtapaAsync(Guid id, Guid etapaId, Guid idUsuario)
    {
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        etapa.Dado!.MarcarConcluida();
        await _repository.AtualizarEtapaAsync(etapa.Dado!);
        return MapearEtapa(etapa.Dado!);
    }

    public async Task<Result<ProcessoEtapaResponse>> EstornarEtapaAsync(Guid id, Guid etapaId, Guid idUsuario)
    {
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        etapa.Dado!.Estornar();
        await _repository.AtualizarEtapaAsync(etapa.Dado!);
        return MapearEtapa(etapa.Dado!);
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
        await _repository.AtualizarEtapaAsync(atual);
        await _repository.AtualizarEtapaAsync(vizinha);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ExcluirEtapaAsync(Guid id, Guid etapaId, Guid idUsuario)
    {
        var etapa = await ObterEtapaDoProcessoAsync(id, etapaId, idUsuario);
        if (!etapa.EhSucesso)
            return etapa.Erro!;

        await _repository.ExcluirEtapaAsync(etapaId);
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

    private static ProcessoResponse Mapear(ProcessoProjecao p, IEnumerable<ProcessoEtapa>? etapas)
    {
        var lista = etapas?.OrderBy(e => e.Ordem).Select(MapearEtapa).ToList() ?? new List<ProcessoEtapaResponse>();
        return new ProcessoResponse
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            IdParceria = p.IdParceria,
            IdContrato = p.IdContrato,
            VinculoTipo = p.VinculoTipo,
            VinculoNome = p.VinculoNome,
            Cliente = p.Cliente,
            Ativo = p.Ativo,
            DataCadastro = p.DataCadastro,
            TotalEtapas = p.TotalEtapas,
            EtapasConcluidas = p.EtapasConcluidas,
            PercentualConcluido = p.TotalEtapas == 0 ? 0 : (int)Math.Round(p.EtapasConcluidas * 100m / p.TotalEtapas),
            Etapas = lista
        };
    }

    private static ProcessoEtapaResponse MapearEtapa(ProcessoEtapa e)
        => new()
        {
            Id = e.Id,
            Nome = e.Nome,
            Descricao = e.Descricao,
            Ordem = e.Ordem,
            Concluida = e.Concluida,
            DataPrevista = e.DataPrevista,
            DataConclusao = e.DataConclusao
        };
}
