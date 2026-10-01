namespace PortalFinanceiro.Core.Domain.Projections;

public class ModeloProcessoProjecao
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataAlteracao { get; set; }
    public Guid? CriadoPor { get; set; }
    public Guid? AlteradoPor { get; set; }
    public int TotalEtapas { get; set; }
    public int TotalItens { get; set; }
}
