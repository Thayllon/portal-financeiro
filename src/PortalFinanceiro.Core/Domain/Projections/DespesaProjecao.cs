using PortalFinanceiro.Core.Domain.Enums;

namespace PortalFinanceiro.Core.Domain.Projections;

public class DespesaProjecao
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public Guid IdConta { get; set; }
    public string Conta { get; set; } = string.Empty;
    public Guid IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public Guid? IdSubcategoria { get; set; }
    public string Subcategoria { get; set; } = string.Empty;
    public StatusMensal Status { get; set; }
    public DateTime? DataRealizacao { get; set; }
    public Guid? IdRegra { get; set; }
    public Guid? IdReceitaOrigem { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool EhRecorrente => IdRegra.HasValue;
}
