using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public interface ICategoriaEntity
{
    Guid Id { get; }
    Guid IdUsuario { get; }
    string Nome { get; }
    Guid? CategoriaPaiId { get; }
    bool Ativo { get; }
    DateTime DataCadastro { get; }
    DateTime DataAlteracao { get; }

    Result<Unit> Atualizar(string nome, Guid? categoriaPaiId);
    void Desativar();
}
