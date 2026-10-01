using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Interfaces.Services;
using PortalFinanceiro.Infrastructure.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PortalFinanceiro.API.Seeders;

public class DatabaseSeeder
{
    private static readonly string[] ModulosPadrao =
    [
        "dashboard", "receitas", "despesas", "contas", "categorias-receita", "categorias-despesa",
        "categorias-servico", "clientes", "parceiros", "parcerias", "contratos"
    ];

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPermissaoUsuarioRepository _permissaoRepository;
    private readonly IPasswordService _passwordService;
    private readonly SeedOptions _seedOptions;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IUsuarioRepository usuarioRepository,
        IPermissaoUsuarioRepository permissaoRepository,
        IPasswordService passwordService,
        SeedOptions seedOptions,
        IHostEnvironment environment,
        ILogger<DatabaseSeeder> logger)
    {
        _usuarioRepository = usuarioRepository;
        _permissaoRepository = permissaoRepository;
        _passwordService = passwordService;
        _seedOptions = seedOptions;
        _environment = environment;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (_seedOptions.Skip)
        {
            _logger.LogInformation("Seed de banco de dados desativado (Seed__Skip=true)");
            return;
        }

        var usuarios = await _usuarioRepository.ListarAsync();
        if (usuarios is not null && usuarios.Any())
            return;

        var senha = await _resolverSenhaInicial();
        if (senha is null)
            return;

        var senhaHash = _passwordService.Hash(senha);
        var usuarioResult = Core.Domain.Entities.Usuario.Criar("Admin", "admin@portal.com", senhaHash, isAdmin: true);

        if (!usuarioResult.EhSucesso)
        {
            _logger.LogError("Falha ao criar usuario admin padrão: {Erro}", usuarioResult.Erro?.Mensagem);
            return;
        }

        await _usuarioRepository.InserirAsync(usuarioResult.Dado!);
        var admin = await _usuarioRepository.ObterPorEmailAsync("admin@portal.com");

        if (admin is null)
        {
            _logger.LogError("Usuario admin criado mas não encontrado para atribuição de permissões");
            return;
        }

        foreach (var modulo in ModulosPadrao)
            await _garantirPermissaoAdmin(admin.Id, modulo);

        _logger.LogInformation("Seed concluído: admin padrão criado com permissões completas");
    }

    private async Task<string?> _resolverSenhaInicial()
    {
        if (_seedOptions.TemSenhaConfigurada())
            return _seedOptions.AdminInitialPassword;

        if (_environment.IsDevelopment())
        {
            _logger.LogWarning("Seed__AdminInitialPassword não configurada; usando senha padrão de desenvolvimento");
            return "senhasenha";
        }

        _logger.LogError("Seed de admin bloqueado: Seed__AdminInitialPassword não configurada fora de Development");
        return null;
    }

    private async Task _garantirPermissaoAdmin(Guid usuarioId, string modulo)
    {
        var existente = await _permissaoRepository.ObterPorUsuarioEModuloAsync(usuarioId, modulo);
        if (existente is not null)
            return;

        var permissao = Core.Domain.Entities.PermissaoUsuario.Criar(
            usuarioId, modulo, Core.Domain.Entities.NivelPermissao.Escrita);

        await _permissaoRepository.InserirAsync(permissao);
    }
}
