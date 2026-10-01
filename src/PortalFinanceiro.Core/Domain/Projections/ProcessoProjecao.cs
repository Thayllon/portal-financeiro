namespace PortalFinanceiro.Core.Domain.Projections;

public class ProcessoProjecao
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Guid? IdParceria { get; set; }
    public Guid? IdContrato { get; set; }
    public string VinculoTipo { get; set; } = string.Empty;
    public string VinculoNome { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataAlteracao { get; set; }
    public int TotalEtapas { get; set; }
    public int EtapasConcluidas { get; set; }
}
