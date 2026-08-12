using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class Receita
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateTime Data { get; private set; }
    public Guid IdConta { get; private set; }
    public Guid IdCategoria { get; private set; }
    public Guid? IdSubcategoria { get; private set; }
    public StatusMensal Status { get; private set; }
    public DateTime? DataRealizacao { get; private set; }
    public Guid? IdRegra { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public string Categoria { get; set; } = string.Empty;
    public string Subcategoria { get; set; } = string.Empty;
    public string Conta { get; set; } = string.Empty;
    public bool GeraDas { get; set; }
    public decimal? PercentualDas { get; set; }
    public bool EhRecorrente => IdRegra.HasValue;

    public Receita() { }

    public static Result<Receita> Criar(Guid idUsuario, string descricao, decimal valor, DateTime data, Guid idConta, Guid idCategoria, Guid? idSubcategoria, Guid? idRegra = null)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(descricao))
            return Erro.Validacao("DESCRICAO_OBRIGATORIA", "Descrição é obrigatória.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");
        if (idConta == Guid.Empty)
            return Erro.Validacao("CONTA_OBRIGATORIA", "Conta é obrigatória.");
        if (idCategoria == Guid.Empty)
            return Erro.Validacao("CATEGORIA_OBRIGATORIA", "Categoria é obrigatória.");

        return new Receita
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Descricao = descricao,
            Valor = valor,
            Data = data,
            IdConta = idConta,
            IdCategoria = idCategoria,
            IdSubcategoria = idSubcategoria,
            Status = StatusMensal.Pendente,
            IdRegra = idRegra,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string descricao, decimal valor, DateTime data, Guid idConta, Guid idCategoria, Guid? idSubcategoria)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return Erro.Validacao("DESCRICAO_OBRIGATORIA", "Descrição é obrigatória.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");

        Descricao = descricao;
        Valor = valor;
        Data = data;
        IdConta = idConta;
        IdCategoria = idCategoria;
        IdSubcategoria = idSubcategoria;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public Result<Unit> Receber(DateTime dataRecebimento)
    {
        if (Status == StatusMensal.Realizado)
            return Erro.Negocio("RECEITA_JA_RECEBIDA", "Esta receita já foi recebida.");

        Status = StatusMensal.Realizado;
        DataRealizacao = dataRecebimento;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public Result<Unit> Estornar()
    {
        if (Status == StatusMensal.Pendente)
            return Erro.Negocio("RECEITA_NAO_RECEBIDA", "Esta receita ainda não foi recebida.");

        Status = StatusMensal.Pendente;
        DataRealizacao = null;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }
}
