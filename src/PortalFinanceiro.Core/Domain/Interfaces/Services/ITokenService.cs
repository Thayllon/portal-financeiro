using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Services;

public interface ITokenService
{
    string GerarToken(Usuario usuario);
}
