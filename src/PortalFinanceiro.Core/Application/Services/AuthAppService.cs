using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Constants;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Interfaces.Services;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class AuthAppService : IAuthAppService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPermissaoUsuarioRepository _permissaoRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthAppService(IUsuarioRepository usuarioRepository, IPermissaoUsuarioRepository permissaoRepository, IPasswordService passwordService, ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _permissaoRepository = permissaoRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);
        if (usuario is null)
            return Erro.NaoEncontrado("Usuário");

        if (!_passwordService.Verificar(request.Senha, usuario.SenhaHash))
            return Erro.Validacao("CREDENCIAIS_INVALIDAS", "Dados inválidos.");

        if (!usuario.Ativo)
            return Erro.Negocio("USUARIO_INATIVO", "Usuário inativo.");

        if (EhSenhaTemporaria(usuario.SenhaHash))
            return new LoginResponse
            {
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                IsAdmin = usuario.IsAdmin,
                PrecisaTrocarSenha = true
            };

        return await GerarSessao(usuario);
    }

    public async Task<Result<LoginResponse>> AlterarSenhaAsync(AlterarSenhaRequest request)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);
        if (usuario is null)
            return Erro.NaoEncontrado("Usuário");

        if (!_passwordService.Verificar(request.SenhaAtual, usuario.SenhaHash))
            return Erro.Validacao("CREDENCIAIS_INVALIDAS", "Senha atual inválida.");

        if (!EhSenhaTemporaria(usuario.SenhaHash))
            return Erro.Negocio("SENHA_JA_TROCADA", "A senha já foi alterada anteriormente.");

        if (string.IsNullOrWhiteSpace(request.NovaSenha) || request.NovaSenha.Length < 6)
            return Erro.Validacao("SENHA_FRACA", "A nova senha deve ter no mínimo 6 caracteres.");

        var senhaHash = _passwordService.Hash(request.NovaSenha);
        var result = usuario.Atualizar(usuario.Nome, usuario.Email, senhaHash, usuario.IsAdmin, usuario.Ativo);
        if (!result.EhSucesso)
            return result.Erro!;

        await _usuarioRepository.AtualizarAsync(usuario);
        return await GerarSessao(usuario);
    }

    private bool EhSenhaTemporaria(string senhaHash) =>
        _passwordService.Verificar(SenhasPadrao.PrimeiroAcesso, senhaHash)
        || _passwordService.Verificar(SenhasPadrao.Reset, senhaHash);

    private async Task<LoginResponse> GerarSessao(Usuario usuario)
    {
        var permissoes = (await _permissaoRepository.ObterPorUsuarioIdAsync(usuario.Id)).ToList();
        var token = _tokenService.GerarToken(usuario, permissoes);

        return new LoginResponse
        {
            Token = token,
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            IsAdmin = usuario.IsAdmin,
            PrecisaTrocarSenha = false,
            DataExpiracao = DateTime.UtcNow.AddHours(_tokenService.ExpirationHours),
            Permissoes = permissoes.Select(p => new Application.Dtos.Response.PermissaoUsuarioResponse
            {
                Modulo = p.Modulo,
                Nivel = (int)p.Nivel
            })
        };
    }
}