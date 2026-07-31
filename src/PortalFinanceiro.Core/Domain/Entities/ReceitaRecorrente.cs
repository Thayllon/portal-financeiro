using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ReceitaRecorrente
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public int Dia { get; private set; }
    public bool DiaUtil { get; private set; }
    public Guid IdCategoria { get; private set; }
    public Guid IdConta { get; private set; }
    public string Categoria { get; set; } = string.Empty;
    public string Conta { get; set; } = string.Empty;
    public DateTime DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public ReceitaRecorrente() { }

    public static Result<ReceitaRecorrente> Criar(Guid idUsuario, string descricao, decimal valor, int dia, bool diaUtil, Guid idCategoria, Guid idConta, DateTime dataInicio, DateTime? dataFim)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(descricao))
            return Erro.Validacao("DESCRICAO_OBRIGATORIA", "Descrição é obrigatória.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");
        if (diaUtil && (dia < 1 || dia > 5))
            return Erro.Validacao("DIA_UTIL_INVALIDO", "Dia útil deve estar entre 1 e 5.");
        if (!diaUtil && (dia < 1 || dia > 31))
            return Erro.Validacao("DIA_INVALIDO", "Dia deve estar entre 1 e 31.");
        if (idCategoria == Guid.Empty)
            return Erro.Validacao("CATEGORIA_OBRIGATORIA", "Categoria é obrigatória.");
        if (idConta == Guid.Empty)
            return Erro.Validacao("CONTA_OBRIGATORIA", "Conta é obrigatória.");
        if (dataFim.HasValue && dataFim.Value < dataInicio)
            return Erro.Validacao("PERIODO_INVALIDO", "Data fim não pode ser anterior à data início.");

        return new ReceitaRecorrente
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Descricao = descricao,
            Valor = valor,
            Dia = dia,
            DiaUtil = diaUtil,
            IdCategoria = idCategoria,
            IdConta = idConta,
            DataInicio = dataInicio,
            DataFim = dataFim,
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string descricao, decimal valor, int dia, bool diaUtil, Guid idCategoria, Guid idConta, DateTime dataInicio, DateTime? dataFim)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return Erro.Validacao("DESCRICAO_OBRIGATORIA", "Descrição é obrigatória.");
        if (valor <= 0)
            return Erro.Validacao("VALOR_INVALIDO", "Valor deve ser maior que zero.");
        if (diaUtil && (dia < 1 || dia > 5))
            return Erro.Validacao("DIA_UTIL_INVALIDO", "Dia útil deve estar entre 1 e 5.");
        if (!diaUtil && (dia < 1 || dia > 31))
            return Erro.Validacao("DIA_INVALIDO", "Dia deve estar entre 1 e 31.");
        if (dataFim.HasValue && dataFim.Value < dataInicio)
            return Erro.Validacao("PERIODO_INVALIDO", "Data fim não pode ser anterior à data início.");

        Descricao = descricao;
        Valor = valor;
        Dia = dia;
        DiaUtil = diaUtil;
        IdCategoria = idCategoria;
        IdConta = idConta;
        DataInicio = dataInicio;
        DataFim = dataFim;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void Desativar()
    {
        Ativo = false;
        DataAlteracao = DateTime.UtcNow;
    }
}
