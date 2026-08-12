namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class CategoriaResponse
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid? CategoriaPaiId { get; set; }
    public bool Ativo { get; set; }
    public bool PodeEditar { get; set; }
    public DateTime DataCadastro { get; set; }
}
