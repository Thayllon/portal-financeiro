using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class ContaBancariaAppService : IContaBancariaAppService
{
    private readonly IContaBancariaRepository _repository;

    public ContaBancariaAppService(IContaBancariaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ContaBancariaResponse>>> ListarAsync(Guid idUsuario)
    {
        var contas = await _repository.ListarPorUsuarioAsync(idUsuario);
        return contas.Select(Mapear).ToList();
    }

    public async Task<Result<ContaBancariaResponse>> ObterPorIdAsync(Guid id)
    {
        var conta = await _repository.ObterPorIdAsync(id);
        if (conta is null)
            return Erro.NaoEncontrado("Conta bancária");

        return Mapear(conta);
    }

    public async Task<Result<ContaBancariaResponse>> AdicionarAsync(Guid idUsuario, ContaBancariaRequest request)
    {
        var result = ContaBancaria.Criar(idUsuario, request.Nome, request.Banco, request.Tipo);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.InserirAsync(result.Dado!);
        return Mapear(result.Dado!);
    }

    public async Task<Result<ContaBancariaResponse>> AtualizarAsync(Guid id, ContaBancariaRequest request)
    {
        var conta = await _repository.ObterPorIdAsync(id);
        if (conta is null)
            return Erro.NaoEncontrado("Conta bancária");

        var result = conta.Atualizar(request.Nome, request.Banco, request.Tipo);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(conta);
        return Mapear(conta);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var conta = await _repository.ObterPorIdAsync(id);
        if (conta is null)
            return Erro.NaoEncontrado("Conta bancária");

        conta.Desativar();
        await _repository.AtualizarAsync(conta);
        return Resultado.Sucesso();
    }

    private static ContaBancariaResponse Mapear(ContaBancaria c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Banco = c.Banco,
        Tipo = c.Tipo.ToString(),
        Ativo = c.Ativo,
        DataCadastro = c.DataCadastro
    };
}
