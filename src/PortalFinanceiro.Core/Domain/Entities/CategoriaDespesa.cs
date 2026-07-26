using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class CategoriaDespesa
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public CategoriaDespesa() { }

    public static Result<CategoriaDespesa> Criar(Guid idUsuario, string nome)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");

        return new CategoriaDespesa
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Nome = nome,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");

        Nome = nome;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }
}
