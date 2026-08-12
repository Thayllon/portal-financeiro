using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class ProLaboreAppService : IProLaboreAppService
{
    private readonly IProLaboreRepository _repository;
    private readonly IEncargoFiscalService _encargoFiscalService;

    public ProLaboreAppService(IProLaboreRepository repository, IEncargoFiscalService encargoFiscalService)
    {
        _repository = repository;
        _encargoFiscalService = encargoFiscalService;
    }

    public async Task<Result<IEnumerable<ProLaboreResponse>>> ListarAsync(Guid idUsuario)
    {
        var registros = await _repository.ListarPorUsuarioAsync(idUsuario);
        return registros.Select(Mapear).ToList();
    }

    public async Task<Result<ProLaboreResponse>> AdicionarAsync(Guid idUsuario, ProLaboreRequest request)
    {
        if (request is null)
            return Erro.Validacao("REQUISICAO_INVALIDA", "Corpo da requisição é obrigatório.");

        var existente = await _repository.ObterPorUsuarioMesAsync(idUsuario, request.Mes, request.Ano);
        if (existente is not null)
            return Erro.Conflito("PRO_LABORE_JA_EXISTE", "Já existe um pró-labore cadastrado para este mês/ano.");

        var result = ProLabore.Criar(idUsuario, request.Ano, request.Mes, request.Valor, request.PercentualInss, request.IdConta);
        if (!result.EhSucesso)
            return result.Erro!;

        var proLabore = result.Dado!;
        await _repository.InserirAsync(proLabore);

        var inss = await _encargoFiscalService.GerarInssAsync(idUsuario, proLabore, proLabore.PercentualInss);
        if (!inss.EhSucesso)
        {
            proLabore.Desativar();
            await _repository.AtualizarAsync(proLabore);
            return inss.Erro!;
        }

        return Mapear(proLabore);
    }

    public async Task<Result<ProLaboreResponse>> AtualizarAsync(Guid id, Guid idUsuario, ProLaboreRequest request)
    {
        var proLabore = await _repository.ObterPorIdAsync(id);
        if (proLabore is null)
            return Erro.NaoEncontrado("Pró-labore");

        if (proLabore.IdUsuario != idUsuario)
            return Erro.Permissao("SEM_PERMISSAO", "Você não tem permissão para editar este pró-labore.");

        if (request is null)
            return Erro.Validacao("REQUISICAO_INVALIDA", "Corpo da requisição é obrigatório.");

        var result = proLabore.Atualizar(request.Valor, request.PercentualInss, request.IdConta);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(proLabore);

        await _encargoFiscalService.RemoverInssAsync(idUsuario, proLabore);
        var inss = await _encargoFiscalService.GerarInssAsync(idUsuario, proLabore, proLabore.PercentualInss);
        if (!inss.EhSucesso)
            return inss.Erro!;

        return Mapear(proLabore);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var proLabore = await _repository.ObterPorIdAsync(id);
        if (proLabore is null)
            return Erro.NaoEncontrado("Pró-labore");

        proLabore.Desativar();
        await _repository.AtualizarAsync(proLabore);
        await _encargoFiscalService.RemoverInssAsync(proLabore.IdUsuario, proLabore);

        return Resultado.Sucesso();
    }

    private static ProLaboreResponse Mapear(ProLabore p) => new()
    {
        Id = p.Id,
        Ano = p.Ano,
        Mes = p.Mes,
        Valor = p.Valor,
        PercentualInss = p.PercentualInss,
        IdConta = p.IdConta,
        Conta = p.Conta,
        Ativo = p.Ativo,
        DataCadastro = p.DataCadastro
    };
}