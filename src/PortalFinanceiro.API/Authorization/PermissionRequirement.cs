using Microsoft.AspNetCore.Authorization;

namespace PortalFinanceiro.API.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Modulo { get; }
    public int NivelMinimo { get; }

    public PermissionRequirement(string modulo, int nivelMinimo)
    {
        Modulo = modulo;
        NivelMinimo = nivelMinimo;
    }
}
