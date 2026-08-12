using PortalFinanceiro.Core.Domain.Enums;

namespace PortalFinanceiro.Core.Domain.Entities;

public class CategoriaHistorico
{
    public Guid Id { get; private set; }
    public Guid IdCategoria { get; private set; }
    public ETipoCategoria TipoCategoria { get; private set; }
    public Guid IdUsuario { get; private set; }
    public EAcaoCategoriaHistorico Acao { get; private set; }
    public string? NomeAntigo { get; private set; }
    public string? NomeNovo { get; private set; }
    public Guid? CategoriaPaiIdAntiga { get; private set; }
    public Guid? CategoriaPaiIdNova { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public CategoriaHistorico() { }

    public static CategoriaHistorico Criar(
        Guid idCategoria,
        ETipoCategoria tipoCategoria,
        Guid idUsuario,
        EAcaoCategoriaHistorico acao,
        string? nomeAntigo = null,
        string? nomeNovo = null,
        Guid? categoriaPaiIdAntiga = null,
        Guid? categoriaPaiIdNova = null)
    {
        return new CategoriaHistorico
        {
            Id = Guid.NewGuid(),
            IdCategoria = idCategoria,
            TipoCategoria = tipoCategoria,
            IdUsuario = idUsuario,
            Acao = acao,
            NomeAntigo = nomeAntigo,
            NomeNovo = nomeNovo,
            CategoriaPaiIdAntiga = categoriaPaiIdAntiga,
            CategoriaPaiIdNova = categoriaPaiIdNova,
            DataCadastro = DateTime.UtcNow
        };
    }
}
