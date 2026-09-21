namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class ContratoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public decimal TotalRecebido { get; set; }
    public decimal FaltaReceber { get; set; }
}
