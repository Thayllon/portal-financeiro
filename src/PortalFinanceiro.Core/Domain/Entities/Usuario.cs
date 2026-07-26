using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public Usuario() { }

    public static Result<Usuario> Criar(string nome, string email, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email))
            return Erro.Validacao("EMAIL_OBRIGATORIO", "Email é obrigatório.");
        if (string.IsNullOrWhiteSpace(senhaHash))
            return Erro.Validacao("SENHA_OBRIGATORIA", "Senha é obrigatória.");

        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            SenhaHash = senhaHash,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email))
            return Erro.Validacao("EMAIL_OBRIGATORIO", "Email é obrigatório.");

        Nome = nome;
        Email = email;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }
}
