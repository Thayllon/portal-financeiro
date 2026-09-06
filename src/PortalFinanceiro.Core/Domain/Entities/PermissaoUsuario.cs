namespace PortalFinanceiro.Core.Domain.Entities;

public class PermissaoUsuario
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Modulo { get; private set; } = string.Empty;
    public NivelPermissao Nivel { get; private set; }

    public PermissaoUsuario() { }

    public static PermissaoUsuario Criar(Guid usuarioId, string modulo, NivelPermissao nivel)
    {
        return new PermissaoUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Modulo = modulo,
            Nivel = nivel
        };
    }

    public void AtualizarNivel(NivelPermissao nivel)
    {
        Nivel = nivel;
    }
}
