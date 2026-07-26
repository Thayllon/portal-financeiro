namespace PortalFinanceiro.Infrastructure.Extensions;

public class AuthOptions
{
    public const string SectionName = "Auth";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "PortalFinanceiroAPI";
    public string Audience { get; set; } = "PortalFinanceiroWeb";
    public int ExpirationHours { get; set; } = 8;
}
