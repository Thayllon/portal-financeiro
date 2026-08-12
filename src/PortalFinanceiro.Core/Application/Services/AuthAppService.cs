using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Interfaces.Services;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class AuthAppService : IAuthAppService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthAppService(IUsuarioRepository usuarioRepository, IPasswordService passwordService, ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);
        if (usuario is null)
            return Erro.NaoEncontrado("Usuário");

        if (!_passwordService.Verificar(request.Senha, usuario.SenhaHash))
            return Erro.Validacao("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos.");

        if (!usuario.Ativo)
            return Erro.Negocio("USUARIO_INATIVO", "Usuário inativo.");

        var token = _tokenService.GerarToken(usuario);

        return new LoginResponse
        {
            Token = token,
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            IsAdmin = usuario.IsAdmin,
            DataExpiracao = DateTime.UtcNow.AddHours(_tokenService.ExpirationHours)
        };
    }
}
