namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class CategoriaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}
