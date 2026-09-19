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

    public async Task<Result<ContaBancariaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var conta = await _repository.ObterPorIdAsync(id);
        if (conta is null)
            return Erro.NaoEncontrado("Conta bancária");
        if (conta.IdUsuario != idUsuario)
            return Erro.Permissao("CONTA_ACESSO_NEGADO", "Conta de outro usuário.");

        return Mapear(conta);
    }

    public async Task<Result<ContaBancariaResponse>> AdicionarAsync(Guid idUsuario, ContaBancariaRequest request)
    {
        var result = ContaBancaria.Criar(idUsuario, request.Nome, request.Banco, request.Tipo);
        if (!result.EhSucesso)
            return result.Erro!;

        var conta = result.Dado!;
        var contas = await _repository.ListarPorUsuarioAsync(idUsuario);
        if (!contas.Any())
            conta.DefinirComoPadrao();

        await _repository.InserirAsync(conta);
        return Mapear(conta);
    }

    public async Task<Result<ContaBancariaResponse>> AtualizarAsync(Guid id, Guid idUsuario, ContaBancariaRequest request)
    {
        var conta = await _repository.ObterPorIdAsync(id);
        if (conta is null)
            return Erro.NaoEncontrado("Conta bancária");
        if (conta.IdUsuario != idUsuario)
            return Erro.Permissao("CONTA_ACESSO_NEGADO", "Conta de outro usuário.");

        var result = conta.Atualizar(request.Nome, request.Banco, request.Tipo);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(conta);
        return Mapear(conta);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var conta = await _repository.ObterPorIdAsync(id);
        if (conta is null)
            return Erro.NaoEncontrado("Conta bancária");
        if (conta.IdUsuario != idUsuario)
            return Erro.Permissao("CONTA_ACESSO_NEGADO", "Conta de outro usuário.");

        var receitas = await _repository.ContarReceitasAsync(id);
        var despesas = await _repository.ContarDespesasAsync(id);

        if (receitas > 0 || despesas > 0)
        {
            var detalhes = new List<string>();
            if (receitas > 0) detalhes.Add($"{receitas} receita(s)");
            if (despesas > 0) detalhes.Add($"{despesas} despesa(s)");
            return Erro.Negocio("CONTA_COM_VINCULOS", $"Não é possível excluir. Existem {string.Join(" e ", detalhes)} vinculadas a esta conta.");
        }

        conta.Desativar();
        await _repository.AtualizarAsync(conta);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> DefinirPadraoAsync(Guid id, Guid idUsuario)
    {
        var conta = await _repository.ObterPorIdAsync(id);
        if (conta is null)
            return Erro.NaoEncontrado("Conta bancária");
        if (conta.IdUsuario != idUsuario)
            return Erro.Permissao("CONTA_ACESSO_NEGADO", "Conta de outro usuário.");

        await _repository.LimparPadraoAsync(idUsuario);
        conta.DefinirComoPadrao();
        await _repository.AtualizarAsync(conta);
        return Resultado.Sucesso();
    }

    private static ContaBancariaResponse Mapear(ContaBancaria c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Banco = c.Banco,
        Tipo = c.Tipo.ToString(),
        EhPadrao = c.EhPadrao,
        Ativo = c.Ativo,
        DataCadastro = c.DataCadastro
    };
}
