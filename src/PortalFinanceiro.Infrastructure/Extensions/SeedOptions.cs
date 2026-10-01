namespace PortalFinanceiro.Infrastructure.Extensions;

public class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminInitialPassword { get; set; } = string.Empty;
    public bool Skip { get; set; }

    public bool TemSenhaConfigurada() => !string.IsNullOrWhiteSpace(AdminInitialPassword);
}
