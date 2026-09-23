using FluentAssertions;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Interfaces.Services;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Aplicacao")]
public class UsuarioAppServiceTests
{
    private sealed class UsuarioRepositoryFake : IUsuarioRepository
    {
        public Usuario? Usuario { get; set; }
        public int Vinculos { get; set; }
        public List<Guid> Excluidos { get; } = new();

        public Task<Usuario?> ObterPorIdAsync(Guid id)
            => Task.FromResult(Usuario?.Id == id ? Usuario : null);

        public Task<Usuario?> ObterPorEmailAsync(string email)
            => Task.FromResult<Usuario?>(null);

        public Task<IEnumerable<Usuario>> ListarAsync()
            => Task.FromResult<IEnumerable<Usuario>>(Array.Empty<Usuario>());

        public Task InserirAsync(Usuario entity) => Task.CompletedTask;
        public Task AtualizarAsync(Usuario entity) => Task.CompletedTask;

        public Task ExcluirAsync(Guid id)
        {
            Excluidos.Add(id);
            return Task.CompletedTask;
        }

        public Task<int> ContarVinculosAsync(Guid id) => Task.FromResult(Vinculos);
    }

    private sealed class PermissaoRepositoryFake : IPermissaoUsuarioRepository
    {
        public List<Guid> ExcluidosPorUsuario { get; } = new();

        public Task<IEnumerable<PermissaoUsuario>> ObterPorUsuarioIdAsync(Guid usuarioId)
            => Task.FromResult<IEnumerable<PermissaoUsuario>>(Array.Empty<PermissaoUsuario>());

        public Task<PermissaoUsuario?> ObterPorUsuarioEModuloAsync(Guid usuarioId, string modulo)
            => Task.FromResult<PermissaoUsuario?>(null);

        public Task InserirAsync(PermissaoUsuario entity) => Task.CompletedTask;
        public Task AtualizarAsync(PermissaoUsuario entity) => Task.CompletedTask;
        public Task ExcluirAsync(Guid id) => Task.CompletedTask;

        public Task ExcluirPorUsuarioIdAsync(Guid usuarioId)
        {
            ExcluidosPorUsuario.Add(usuarioId);
            return Task.CompletedTask;
        }
    }

    private sealed class PasswordServiceFake : IPasswordService
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senha, string hash) => hash == $"hash:{senha}";
    }

    private static (UsuarioAppService service, UsuarioRepositoryFake usuarios, PermissaoRepositoryFake permissoes, Usuario alvo) CriarCenario(int vinculos = 0)
    {
        var usuarios = new UsuarioRepositoryFake();
        var permissoes = new PermissaoRepositoryFake();
        var service = new UsuarioAppService(usuarios, permissoes, new PasswordServiceFake());
        var alvo = Usuario.Criar("Maria Silva", "maria@demo.com", "hash").Dado!;
        usuarios.Usuario = alvo;
        usuarios.Vinculos = vinculos;
        return (service, usuarios, permissoes, alvo);
    }

    [Fact]
    public async Task Excluir_ProprioUsuario_RetornaAutoExclusao()
    {
        var (service, usuarios, permissoes, alvo) = CriarCenario();

        var result = await service.ExcluirAsync(alvo.Id, alvo.Id);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("AUTO_EXCLUSAO");
        result.Erro.Tipo.Should().Be(ETipoErro.Negocio);
        usuarios.Excluidos.Should().BeEmpty();
        permissoes.ExcluidosPorUsuario.Should().BeEmpty();
    }

    [Fact]
    public async Task Excluir_UsuarioInexistente_RetornaNaoEncontrado()
    {
        var (service, usuarios, permissoes, _) = CriarCenario();

        var result = await service.ExcluirAsync(Guid.NewGuid(), Guid.NewGuid());

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Tipo.Should().Be(ETipoErro.NaoEncontrado);
        usuarios.Excluidos.Should().BeEmpty();
        permissoes.ExcluidosPorUsuario.Should().BeEmpty();
    }

    [Fact]
    public async Task Excluir_ComVinculos_RetornaNegocioSemExcluir()
    {
        var (service, usuarios, permissoes, alvo) = CriarCenario(vinculos: 7);

        var result = await service.ExcluirAsync(alvo.Id, Guid.NewGuid());

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("USUARIO_COM_VINCULOS");
        result.Erro.Tipo.Should().Be(ETipoErro.Negocio);
        usuarios.Excluidos.Should().BeEmpty();
        permissoes.ExcluidosPorUsuario.Should().BeEmpty();
    }

    [Fact]
    public async Task Excluir_SemVinculos_ExcluiPermissoesEUsuario()
    {
        var (service, usuarios, permissoes, alvo) = CriarCenario();

        var result = await service.ExcluirAsync(alvo.Id, Guid.NewGuid());

        result.EhSucesso.Should().BeTrue();
        permissoes.ExcluidosPorUsuario.Should().ContainSingle().Which.Should().Be(alvo.Id);
        usuarios.Excluidos.Should().ContainSingle().Which.Should().Be(alvo.Id);
    }
}
