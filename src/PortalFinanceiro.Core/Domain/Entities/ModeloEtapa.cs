using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ModeloEtapa
{
    public Guid Id { get; private set; }
    public Guid IdModeloProcesso { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int Ordem { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }
    public Guid? CriadoPor { get; private set; }
    public Guid? AlteradoPor { get; private set; }

    public void DefinirCriador(Guid ator) { CriadoPor ??= ator; }
    public void DefinirEditor(Guid ator) { AlteradoPor = ator; DataAlteracao = DateTime.UtcNow; }

    public ModeloEtapa() { }

    public static Result<ModeloEtapa> Criar(Guid idModeloProcesso, string nome, string? descricao, int ordem)
    {
        if (idModeloProcesso == Guid.Empty)
            return Erro.Validacao("MODELO_OBRIGATORIO", "Modelo de processo é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (ordem < 1)
            return Erro.Validacao("ORDEM_INVALIDA", "Ordem deve ser maior que zero.");

        return new ModeloEtapa
        {
            Id = Guid.NewGuid(),
            IdModeloProcesso = idModeloProcesso,
            Nome = nome,
            Descricao = descricao,
            Ordem = ordem,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string nome, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");

        Nome = nome;
        Descricao = descricao;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void MoverPara(int novaOrdem)
    {
        Ordem = novaOrdem;
        DataAlteracao = DateTime.UtcNow;
    }
}
