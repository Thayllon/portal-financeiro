namespace PortalFinanceiro.Core.Domain.Services;

public static class EncargoFiscal
{
    public const decimal PercentualDasPadrao = 6m;
    public const string CategoriaCnpj = "CNPJ";
    public const string CategoriaDas = "DAS";
    public const string DescricaoDas = "DAS";

    public static decimal Calcular(decimal baseValor, decimal percentual)
        => Math.Round(baseValor * percentual / 100m, 2);
}
