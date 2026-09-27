namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ProcessoEtapaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime? DataPrevista { get; set; }
}
