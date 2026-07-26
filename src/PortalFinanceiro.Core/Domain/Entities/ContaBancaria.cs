using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ContaBancaria
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Banco { get; private set; } = string.Empty;
    public TipoConta Tipo { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public ContaBancaria() { }

    public static Result<ContaBancaria> Criar(Guid idUsuario, string nome, string banco, TipoConta tipo)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(banco))
            return Erro.Validacao("BANCO_OBRIGATORIO", "Banco é obrigatório.");

        return new ContaBancaria
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Nome = nome,
            Banco = banco,
            Tipo = tipo,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string nome, string banco, TipoConta tipo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(banco))
            return Erro.Validacao("BANCO_OBRIGATORIO", "Banco é obrigatório.");

        Nome = nome;
        Banco = banco;
        Tipo = tipo;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }
}
