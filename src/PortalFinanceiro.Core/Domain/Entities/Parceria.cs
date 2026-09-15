using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class Parceria
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public Guid IdParceiro { get; private set; }
    public Guid IdCliente { get; private set; }
    public decimal Valor { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public Parceria() { }

    public static Result<Parceria> Criar(Guid idUsuario, Guid idParceiro, Guid idCliente, decimal valor)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (idParceiro == Guid.Empty)
            return Erro.Validacao("PARCEIRO_OBRIGATORIO", "Parceiro é obrigatório.");
        if (idCliente == Guid.Empty)
            return Erro.Validacao("CLIENTE_OBRIGATORIO", "Cliente é obrigatório.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");

        return new Parceria
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            IdParceiro = idParceiro,
            IdCliente = idCliente,
            Valor = valor,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(Guid idParceiro, Guid idCliente, decimal valor)
    {
        if (idParceiro == Guid.Empty)
            return Erro.Validacao("PARCEIRO_OBRIGATORIO", "Parceiro é obrigatório.");
        if (idCliente == Guid.Empty)
            return Erro.Validacao("CLIENTE_OBRIGATORIO", "Cliente é obrigatório.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");

        IdParceiro = idParceiro;
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
}
