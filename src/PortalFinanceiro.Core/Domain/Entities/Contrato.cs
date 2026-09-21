using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class Contrato
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public Guid IdCliente { get; private set; }
    public decimal Valor { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public Contrato() { }

    public static Result<Contrato> Criar(Guid idUsuario, string nome, Guid idCliente, decimal valor)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (idCliente == Guid.Empty)
            return Erro.Validacao("CLIENTE_OBRIGATORIO", "Cliente é obrigatório.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");

        return new Contrato
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Nome = nome,
            IdCliente = idCliente,
            Valor = valor,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string nome, Guid idCliente, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (idCliente == Guid.Empty)
            return Erro.Validacao("CLIENTE_OBRIGATORIO", "Cliente é obrigatório.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");

        Nome = nome;
        IdCliente = idCliente;
        Valor = valor;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        DataAlteracao = DateTime.UtcNow;
    }
}
