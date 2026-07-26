using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ReceitaMensal
{
    public Guid Id { get; private set; }
    public Guid IdReceitaRecorrente { get; private set; }
    public int Mes { get; private set; }
    public int Ano { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime? DataRecebimento { get; private set; }
    public StatusMensal Status { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public ReceitaMensal() { }

    public static Result<ReceitaMensal> Criar(Guid idReceitaRecorrente, int mes, int ano, decimal valor)
    {
        if (idReceitaRecorrente == Guid.Empty)
            return Erro.Validacao("RECEITA_OBRIGATORIA", "Receita recorrente é obrigatória.");
        if (mes < 1 || mes > 12)
            return Erro.Validacao("MES_INVALIDO", "Mês inválido.");
        if (ano < 2000)
            return Erro.Validacao("ANO_INVALIDO", "Ano inválido.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");

        return new ReceitaMensal
        {
            Id = Guid.NewGuid(),
            IdReceitaRecorrente = idReceitaRecorrente,
            Mes = mes,
            Ano = ano,
            Valor = valor,
            Status = StatusMensal.Pendente,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Receber(DateTime dataRecebimento)
    {
        if (Status == StatusMensal.Realizado)
            return Erro.Negocio("RECEITA_JA_RECEBIDA", "Esta receita já foi recebida.");

        Status = StatusMensal.Realizado;
        DataRecebimento = dataRecebimento;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public Result<Unit> Estornar()
    {
        if (Status == StatusMensal.Pendente)
            return Erro.Negocio("RECEITA_NAO_RECEBIDA", "Esta receita ainda não foi recebida.");

        Status = StatusMensal.Pendente;
        DataRecebimento = null;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }
}
