namespace PortalFinanceiro.Core.Domain.Projections;

public class ProcessoProjecao
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Guid? IdParceria { get; set; }
    public Guid? IdContrato { get; set; }
    public Guid? IdModeloProcesso { get; set; }
    public string ModeloNome { get; set; } = string.Empty;
    public Guid? IdCliente { get; set; }
    public string VinculoTipo { get; set; } = string.Empty;
    public string VinculoNome { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime? DataEncerramento { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataAlteracao { get; set; }
    public Guid? CriadoPor { get; set; }
    public Guid? AlteradoPor { get; set; }
    public string FaseAtual { get; set; } = string.Empty;
    public int TotalEtapas { get; set; }
    public int EtapasConcluidas { get; set; }
    public int TotalItens { get; set; }
    public int ItensConcluidos { get; set; }
}
