namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class ModeloItemResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Obrigatorio { get; set; }
    public bool ExigeAnexo { get; set; }
    public int Ordem { get; set; }
    public Guid? CriadoPor { get; set; }
    public Guid? AlteradoPor { get; set; }
}

public class ModeloEtapaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
    public Guid? CriadoPor { get; set; }
    public Guid? AlteradoPor { get; set; }
    public List<ModeloItemResponse> Itens { get; set; } = new();
}

public class ModeloProcessoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public Guid? CriadoPor { get; set; }
    public Guid? AlteradoPor { get; set; }
    public int TotalEtapas { get; set; }
    public int TotalItens { get; set; }
    public List<ModeloEtapaResponse> Etapas { get; set; } = new();
}
