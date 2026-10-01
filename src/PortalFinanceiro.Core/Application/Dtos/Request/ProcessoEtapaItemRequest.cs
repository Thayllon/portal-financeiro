namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class ProcessoEtapaItemRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Obrigatorio { get; set; } = true;
    public bool ExigeAnexo { get; set; } = false;
}
