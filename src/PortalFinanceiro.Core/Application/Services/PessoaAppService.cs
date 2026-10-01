using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PortalFinanceiro.Core.Application.Services;

public class PessoaAppService : IPessoaAppService
{
    private readonly IPessoaRepository _repository;
    private readonly ILogger<PessoaAppService> _logger;

    public PessoaAppService(IPessoaRepository repository, ILogger<PessoaAppService>? logger = null)
    {
        _repository = repository;
        _logger = logger ?? NullLogger<PessoaAppService>.Instance;
    }

    public async Task<Result<IEnumerable<PessoaResponse>>> ListarAsync(Guid idUsuario)
    {
        var pessoas = await _repository.ListarPorUsuarioAsync(idUsuario);
        return pessoas.Select(Mapear).ToList();
    }

    public async Task<Result<PessoaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var pessoa = await _repository.ObterPorIdAsync(id);
        if (pessoa is null)
            return Erro.NaoEncontrado("Pessoa");
        if (pessoa.IdUsuario != idUsuario)
            return Erro.Permissao("PESSOA_ACESSO_NEGADO", "Pessoa de outro usuário.");

        return Mapear(pessoa);
    }

    public async Task<Result<PessoaResponse>> AdicionarAsync(Guid idUsuario, PessoaRequest request)
    {
        var result = Pessoa.Criar(idUsuario, request.Nome, request.Telefone, request.Tipo);
        if (!result.EhSucesso)
            return result.Erro!;

        var pessoa = result.Dado!;
        pessoa.DefinirCriador(idUsuario);
        await _repository.InserirAsync(pessoa);
        _logger.Auditar(idUsuario, "Criar", "Pessoa", pessoa.Id);
        return Mapear(pessoa);
    }

    public async Task<Result<PessoaResponse>> AtualizarAsync(Guid id, Guid idUsuario, PessoaRequest request)
    {
        var pessoa = await _repository.ObterPorIdAsync(id);
        if (pessoa is null)
            return Erro.NaoEncontrado("Pessoa");
        if (pessoa.IdUsuario != idUsuario)
            return Erro.Permissao("PESSOA_ACESSO_NEGADO", "Pessoa de outro usuário.");

        var result = pessoa.Atualizar(request.Nome, request.Telefone, request.Tipo);
        if (!result.EhSucesso)
            return result.Erro!;

        pessoa.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(pessoa);
        _logger.Auditar(idUsuario, "Atualizar", "Pessoa", pessoa.Id);
        return Mapear(pessoa);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var pessoa = await _repository.ObterPorIdAsync(id);
        if (pessoa is null)
            return Erro.NaoEncontrado("Pessoa");
        if (pessoa.IdUsuario != idUsuario)
            return Erro.Permissao("PESSOA_ACESSO_NEGADO", "Pessoa de outro usuário.");

        pessoa.Desativar();
        pessoa.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(pessoa);
        _logger.Auditar(idUsuario, "Excluir", "Pessoa", pessoa.Id);
        return Resultado.Sucesso();
    }

    private static PessoaResponse Mapear(Pessoa p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Telefone = p.Telefone,
        Tipo = p.Tipo.ToString(),
        Ativo = p.Ativo,
        DataCadastro = p.DataCadastro,
        CriadoPor = p.CriadoPor,
        AlteradoPor = p.AlteradoPor
    };
}