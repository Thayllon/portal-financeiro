using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class Parceria
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public Guid IdParceiro { get; private set; }
    public Guid IdCliente { get; private set; }
    public decimal Valor { get; private set; }
    public decimal PercentualParceiro { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public Parceria() { }

    public static Result<Parceria> Criar(Guid idUsuario, string nome, Guid idParceiro, Guid idCliente, decimal valor, decimal percentualParceiro)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (idParceiro == Guid.Empty)
            return Erro.Validacao("PARCEIRO_OBRIGATORIO", "Parceiro é obrigatório.");
        if (idCliente == Guid.Empty)
            return Erro.Validacao("CLIENTE_OBRIGATORIO", "Cliente é obrigatório.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");
        if (percentualParceiro < 0 || percentualParceiro > 100)
            return Erro.Validacao("PERCENTUAL_INVALIDO", "Percentual do parceiro deve estar entre 0 e 100.");

        return new Parceria
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Nome = nome,
            IdParceiro = idParceiro,
            IdCliente = idCliente,
            Valor = valor,
            PercentualParceiro = percentualParceiro,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string nome, Guid idParceiro, Guid idCliente, decimal valor, decimal percentualParceiro)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (idParceiro == Guid.Empty)
            return Erro.Validacao("PARCEIRO_OBRIGATORIO", "Parceiro é obrigatório.");
        if (idCliente == Guid.Empty)
            return Erro.Validacao("CLIENTE_OBRIGATORIO", "Cliente é obrigatório.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");
        if (percentualParceiro < 0 || percentualParceiro > 100)
            return Erro.Validacao("PERCENTUAL_INVALIDO", "Percentual do parceiro deve estar entre 0 e 100.");

        Nome = nome;
        IdParceiro = idParceiro;
        IdCliente = idCliente;
        Valor = valor;
        PercentualParceiro = percentualParceiro;
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
