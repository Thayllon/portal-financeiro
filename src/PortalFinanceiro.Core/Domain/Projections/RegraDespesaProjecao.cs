namespace PortalFinanceiro.Core.Domain.Projections;

public class RegraDespesaProjecao
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Dia { get; set; }
    public bool DiaUtil { get; set; }
    public Guid IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public Guid IdConta { get; set; }
    public string Conta { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public bool Ativo { get; set; }
}
