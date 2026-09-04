using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class PessoaAppService : IPessoaAppService
{
    private readonly IPessoaRepository _repository;

    public PessoaAppService(IPessoaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<PessoaResponse>>> ListarAsync(Guid idUsuario)
    {
        var pessoas = await _repository.ListarPorUsuarioAsync(idUsuario);
        return pessoas.Select(Mapear).ToList();
    }

    public async Task<Result<PessoaResponse>> ObterPorIdAsync(Guid id)
    {
        var pessoa = await _repository.ObterPorIdAsync(id);
        if (pessoa is null)
            return Erro.NaoEncontrado("Pessoa");

        return Mapear(pessoa);
    }

    public async Task<Result<PessoaResponse>> AdicionarAsync(Guid idUsuario, PessoaRequest request)
    {
        var result = Pessoa.Criar(idUsuario, request.Nome, request.Telefone, request.Tipo);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.InserirAsync(result.Dado!);
        return Mapear(result.Dado!);
    }

    public async Task<Result<PessoaResponse>> AtualizarAsync(Guid id, PessoaRequest request)
    {
        var pessoa = await _repository.ObterPorIdAsync(id);
        if (pessoa is null)
            return Erro.NaoEncontrado("Pessoa");

        var result = pessoa.Atualizar(request.Nome, request.Telefone, request.Tipo);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(pessoa);
        return Mapear(pessoa);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var pessoa = await _repository.ObterPorIdAsync(id);
        if (pessoa is null)
            return Erro.NaoEncontrado("Pessoa");

        pessoa.Desativar();
        await _repository.AtualizarAsync(pessoa);
        return Resultado.Sucesso();
    }

    private static PessoaResponse Mapear(Pessoa p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Telefone = p.Telefone,
        Tipo = p.Tipo.ToString(),
        Ativo = p.Ativo,
        DataCadastro = p.DataCadastro
    };
}