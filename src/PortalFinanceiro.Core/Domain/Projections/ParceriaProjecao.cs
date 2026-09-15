namespace PortalFinanceiro.Core.Domain.Projections;

public class ParceriaProjecao
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid IdParceiro { get; set; }
    public string Parceiro { get; set; } = string.Empty;
    public Guid IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal PercentualParceiro { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataAlteracao { get; set; }
}
