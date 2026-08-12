namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class ProLaboreResponse
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public decimal Valor { get; set; }
    public decimal PercentualInss { get; set; }
    public Guid IdConta { get; set; }
    public string Conta { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}
