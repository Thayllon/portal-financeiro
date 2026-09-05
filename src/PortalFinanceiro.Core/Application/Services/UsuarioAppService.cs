using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Constants;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Interfaces.Services;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class UsuarioAppService : IUsuarioAppService
{
    private readonly IUsuarioRepository _repository;
    private readonly IPermissaoUsuarioRepository _permissaoRepository;
    private readonly IPasswordService _passwordService;

    public UsuarioAppService(IUsuarioRepository repository, IPermissaoUsuarioRepository permissaoRepository, IPasswordService passwordService)
    {
        _repository = repository;
        _permissaoRepository = permissaoRepository;
        _passwordService = passwordService;
    }

    public async Task<Result<IEnumerable<UsuarioResponse>>> ListarAsync()
    {
        var usuarios = await _repository.ListarAsync();
        return usuarios.Select(Mapear).ToList();
    }

    public async Task<Result<UsuarioResponse>> AdicionarAsync(UsuarioRequest request)
    {
        var existente = await _repository.ObterPorEmailAsync(request.Email);
        if (existente is not null)
            return Erro.Conflito("EMAIL_EXISTENTE", "Este email já está cadastrado.");

        var senhaHash = _passwordService.Hash(SenhasPadrao.PrimeiroAcesso);

        var result = Usuario.Criar(request.Nome, request.Email, senhaHash, request.IsAdmin);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.InserirAsync(result.Dado!);

        var modulos = new[] { "dashboard", "receitas", "despesas", "contas", "categorias", "clientes", "parceiros" };
        foreach (var modulo in modulos)
        {
            var permissao = PermissaoUsuario.Criar(result.Dado!.Id, modulo, NivelPermissao.Nenhum);
            await _permissaoRepository.InserirAsync(permissao);
        }

        return Mapear(result.Dado!);
    }

    public async Task<Result<UsuarioResponse>> AtualizarAsync(Guid id, UsuarioRequest request)
    {
        var usuario = await _repository.ObterPorIdAsync(id);
        if (usuario is null)
            return Erro.NaoEncontrado("Usuário");

        var senhaHash = string.IsNullOrWhiteSpace(request.Senha) ? null : _passwordService.Hash(request.Senha);

        var result = usuario.Atualizar(request.Nome, request.Email, senhaHash, request.IsAdmin, request.Ativo);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(usuario);
        return Mapear(usuario);
    }

    public async Task<Result<Unit>> AlterarAtivoAsync(Guid id, bool ativo)
    {
        var usuario = await _repository.ObterPorIdAsync(id);
        if (usuario is null)
            return Erro.NaoEncontrado("Usuário");

        if (ativo)
        {
            var result = usuario.Atualizar(usuario.Nome, usuario.Email, null, usuario.IsAdmin, true);
            if (!result.EhSucesso)
                return result.Erro!;
        }
        else
        {
            usuario.Desativar();
        }

        await _repository.AtualizarAsync(usuario);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ResetarSenhaAsync(Guid id)
    {
        var usuario = await _repository.ObterPorIdAsync(id);
        if (usuario is null)
            return Erro.NaoEncontrado("Usuário");

        var senhaHash = _passwordService.Hash(SenhasPadrao.Reset);

        var result = usuario.Atualizar(usuario.Nome, usuario.Email, senhaHash, usuario.IsAdmin, usuario.Ativo);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(usuario);
        return Resultado.Sucesso();
    }

    private static UsuarioResponse Mapear(Usuario u) => new()
    {
        Id = u.Id,
        Nome = u.Nome,
        Email = u.Email,
        IsAdmin = u.IsAdmin,
        Ativo = u.Ativo,
        DataCadastro = u.DataCadastro
    };
}
