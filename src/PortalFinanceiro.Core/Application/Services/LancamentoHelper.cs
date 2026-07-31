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

    public static DateTime CalcularDataVencimento(int dia, bool diaUtil, int mes, int ano)
    {
        if (!diaUtil)
        {
            var ultimoDia = DateTime.DaysInMonth(ano, mes);
            return new DateTime(ano, mes, Math.Min(dia, ultimoDia));
        }

        var data = new DateTime(ano, mes, 1);
        var diasUteisEncontrados = 0;

        if (dia == 0)
        {
            data = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes));
            while (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
                data = data.AddDays(-1);
            return data;
        }

        while (diasUteisEncontrados < dia)
        {
            if (data.DayOfWeek != DayOfWeek.Saturday && data.DayOfWeek != DayOfWeek.Sunday)
                diasUteisEncontrados++;
            if (diasUteisEncontrados < dia)
                data = data.AddDays(1);
        }

        return data;
    }
}
