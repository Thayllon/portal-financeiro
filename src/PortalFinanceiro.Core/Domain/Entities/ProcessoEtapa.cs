using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ProcessoEtapa
{
    public Guid Id { get; private set; }
    public Guid IdProcesso { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int Ordem { get; private set; }
    public bool Concluida { get; private set; }
    public DateTime? DataPrevista { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public ProcessoEtapa() { }

    public static Result<ProcessoEtapa> Criar(Guid idProcesso, string nome, string? descricao, int ordem, DateTime? dataPrevista)
    {
        if (idProcesso == Guid.Empty)
            return Erro.Validacao("PROCESSO_OBRIGATORIO", "Processo é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (ordem < 1)
            return Erro.Validacao("ORDEM_INVALIDA", "Ordem deve ser maior que zero.");

        return new ProcessoEtapa
        {
            Id = Guid.NewGuid(),
            IdProcesso = idProcesso,
            Nome = nome,
            Descricao = descricao,
            Ordem = ordem,
            Concluida = false,
            DataPrevista = dataPrevista,
            DataConclusao = null,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string nome, string? descricao, DateTime? dataPrevista)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");

        Nome = nome;
        Descricao = descricao;
        DataPrevista = dataPrevista;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void MarcarConcluida()
    {
        Concluida = true;
        DataConclusao = DateTime.UtcNow;
        DataAlteracao = DateTime.UtcNow;
    }

    public void Estornar()
    {
        Concluida = false;
        DataConclusao = null;
        DataAlteracao = DateTime.UtcNow;
    }

    public void MoverPara(int novaOrdem)
    {
        Ordem = novaOrdem;
        DataAlteracao = DateTime.UtcNow;
    }
}
