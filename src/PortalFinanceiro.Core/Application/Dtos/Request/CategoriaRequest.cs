namespace PortalFinanceiro.Core.Application.Dtos.Request;

public class CategoriaRequest
{
    public string Nome { get; set; } = string.Empty;
    public Guid? CategoriaPaiId { get; set; }
}
