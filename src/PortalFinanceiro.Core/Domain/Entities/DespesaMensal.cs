using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class DespesaMensal
{
    public Guid Id { get; private set; }
    public Guid IdDespesaRecorrente { get; private set; }
    public int Mes { get; private set; }
    public int Ano { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime? DataPagamento { get; private set; }
    public StatusMensal Status { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public DespesaMensal() { }

    public static Result<DespesaMensal> Criar(Guid idDespesaRecorrente, int mes, int ano, decimal valor)
    {
        if (idDespesaRecorrente == Guid.Empty)
            return Erro.Validacao("DESPESA_OBRIGATORIA", "Despesa recorrente é obrigatória.");
        if (mes < 1 || mes > 12)
            return Erro.Validacao("MES_INVALIDO", "Mês inválido.");
        if (ano < 2000)
            return Erro.Validacao("ANO_INVALIDO", "Ano inválido.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");

        return new DespesaMensal
        {
            Id = Guid.NewGuid(),
            IdDespesaRecorrente = idDespesaRecorrente,
            Mes = mes,
            Ano = ano,
            Valor = valor,
            Status = StatusMensal.Pendente,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Pagar(DateTime dataPagamento)
    {
        if (Status == StatusMensal.Realizado)
            return Erro.Negocio("DESPESA_JA_PAGA", "Esta despesa já foi paga.");

        Status = StatusMensal.Realizado;
        DataPagamento = dataPagamento;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public Result<Unit> Estornar()
    {
        if (Status == StatusMensal.Pendente)
            return Erro.Negocio("DESPESA_NAO_PAGA", "Esta despesa ainda não foi paga.");

        Status = StatusMensal.Pendente;
        DataPagamento = null;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }
}
