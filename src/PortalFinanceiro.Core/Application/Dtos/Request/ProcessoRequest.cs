namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ProcessoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Guid? IdParceria { get; set; }
    public Guid? IdContrato { get; set; }
}
