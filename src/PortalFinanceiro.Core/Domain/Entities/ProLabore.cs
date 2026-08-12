using PortalFinanceiro.Core.Domain.Results;
using PortalFinanceiro.Core.Domain.Services;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ProLabore
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public int Ano { get; private set; }
    public int Mes { get; private set; }
    public decimal Valor { get; private set; }
    public decimal PercentualInss { get; private set; }
    public Guid IdConta { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public string Conta { get; set; } = string.Empty;

    public ProLabore() { }

    public static Result<ProLabore> Criar(Guid idUsuario, int ano, int mes, decimal valor, decimal percentualInss, Guid idConta)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (idConta == Guid.Empty)
            return Erro.Validacao("CONTA_OBRIGATORIA", "Conta é obrigatória.");
        if (ano < 2000 || ano > 2100)
            return Erro.Validacao("ANO_INVALIDO", "Ano inválido.");
        if (mes < 1 || mes > 12)
            return Erro.Validacao("MES_INVALIDO", "Mês deve estar entre 1 e 12.");
        if (valor < EncargoFiscal.SalarioMinimo)
            return Erro.Validacao("VALOR_ABAIXO_SALARIO_MINIMO", $"O pró-labore não pode ser menor que o salário mínimo (R$ {EncargoFiscal.SalarioMinimo:F2}).");
        if (percentualInss <= 0 || percentualInss >= 100)
            return Erro.Validacao("PERCENTUAL_INSS_INVALIDO", "O percentual de INSS deve estar entre 0 e 100.");

        return new ProLabore
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Ano = ano,
            Mes = mes,
            Valor = valor,
            PercentualInss = percentualInss,
            IdConta = idConta,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(decimal valor, decimal percentualInss, Guid idConta)
    {
        if (idConta == Guid.Empty)
            return Erro.Validacao("CONTA_OBRIGATORIA", "Conta é obrigatória.");
        if (valor < EncargoFiscal.SalarioMinimo)
            return Erro.Validacao("VALOR_ABAIXO_SALARIO_MINIMO", $"O pró-labore não pode ser menor que o salário mínimo (R$ {EncargoFiscal.SalarioMinimo:F2}).");
        if (percentualInss <= 0 || percentualInss >= 100)
            return Erro.Validacao("PERCENTUAL_INSS_INVALIDO", "O percentual de INSS deve estar entre 0 e 100.");

        Valor = valor;
        PercentualInss = percentualInss;
        IdConta = idConta;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }
}