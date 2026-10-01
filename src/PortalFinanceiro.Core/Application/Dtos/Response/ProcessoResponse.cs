namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class ProcessoEtapaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
    public bool Concluida { get; set; }
    public DateTime? DataPrevista { get; set; }
    public DateTime? DataConclusao { get; set; }
    public Guid? CriadoPor { get; set; }
    public Guid? AlteradoPor { get; set; }
}

public class ProcessoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Guid? IdParceria { get; set; }
    public Guid? IdContrato { get; set; }
    public string VinculoTipo { get; set; } = string.Empty;
    public string VinculoNome { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public Guid? CriadoPor { get; set; }
    public Guid? AlteradoPor { get; set; }
    public int TotalEtapas { get; set; }
    public int EtapasConcluidas { get; set; }
    public int PercentualConcluido { get; set; }
    public List<ProcessoEtapaResponse> Etapas { get; set; } = new();
}
