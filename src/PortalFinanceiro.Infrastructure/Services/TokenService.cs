using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Services;
using PortalFinanceiro.Infrastructure.Extensions;

namespace PortalFinanceiro.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly AuthOptions _options;

    public TokenService(IOptions<AuthOptions> options)
    {
        _options = options.Value;
    }

    public int ExpirationHours => _options.ExpirationHours;

    public string GerarToken(Usuario usuario, IEnumerable<PermissaoUsuario>? permissoes = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.IsAdmin ? "Admin" : "Usuario")
        };

        if (permissoes is not null)
        {
            foreach (var p in permissoes)
            {
                claims.Add(new Claim($"perm:{p.Modulo}", ((int)p.Nivel).ToString()));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_options.ExpirationHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
