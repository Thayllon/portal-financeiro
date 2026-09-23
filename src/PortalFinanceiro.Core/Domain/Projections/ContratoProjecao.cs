namespace PortalFinanceiro.Core.Domain.Projections;

public class ContratoProjecao
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
    public bool EhRecorrente { get; set; }
    public Guid? IdRegra { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataAlteracao { get; set; }
}
