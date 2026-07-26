namespace PortalFinanceiro.Core.Application.Services;

public static class LancamentoHelper
{
    public static List<(int Mes, int Ano)> GerarMeses(DateTime dataInicio, DateTime? dataFim)
    {
        var meses = new List<(int, int)>();
        var fim = dataFim ?? DateTime.UtcNow.AddYears(5);
        var atual = new DateTime(dataInicio.Year, dataInicio.Month, 1);

        while (atual <= fim)
        {
            meses.Add((atual.Month, atual.Year));
            atual = atual.AddMonths(1);
        }

        return meses;
    }
}
