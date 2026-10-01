using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ModeloItem
{
    public Guid Id { get; private set; }
    public Guid IdModeloEtapa { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public bool Obrigatorio { get; private set; }
    public bool ExigeAnexo { get; private set; }
    public int Ordem { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }
    public Guid? CriadoPor { get; private set; }
    public Guid? AlteradoPor { get; private set; }

    public void DefinirCriador(Guid ator) { CriadoPor ??= ator; }
    public void DefinirEditor(Guid ator) { AlteradoPor = ator; DataAlteracao = DateTime.UtcNow; }

    public ModeloItem() { }

    public static Result<ModeloItem> Criar(Guid idModeloEtapa, string nome, string? descricao, bool obrigatorio, bool exigeAnexo, int ordem)
    {
        if (idModeloEtapa == Guid.Empty)
            return Erro.Validacao("ETAPA_OBRIGATORIA", "Fase do modelo é obrigatória.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        if (ordem < 1)
            return Erro.Validacao("ORDEM_INVALIDA", "Ordem deve ser maior que zero.");

        return new ModeloItem
        {
            Id = Guid.NewGuid(),
            IdModeloEtapa = idModeloEtapa,
            Nome = nome,
            Descricao = descricao,
            Obrigatorio = obrigatorio,
            ExigeAnexo = exigeAnexo,
            Ordem = ordem,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public Result<Unit> Atualizar(string nome, string? descricao, bool obrigatorio, bool exigeAnexo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");

        Nome = nome;
        Descricao = descricao;
        Obrigatorio = obrigatorio;
        ExigeAnexo = exigeAnexo;
        DataAlteracao = DateTime.UtcNow;
        return Resultado.Sucesso();
    }

    public void MoverPara(int novaOrdem)
    {
        Ordem = novaOrdem;
        DataAlteracao = DateTime.UtcNow;
    }
}
