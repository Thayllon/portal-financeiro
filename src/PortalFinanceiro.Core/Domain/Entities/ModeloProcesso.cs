using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ModeloProcesso
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }
    public Guid? CriadoPor { get; private set; }
    public Guid? AlteradoPor { get; private set; }

    public void DefinirCriador(Guid ator) { CriadoPor ??= ator; }
    public void DefinirEditor(Guid ator) { AlteradoPor = ator; DataAlteracao = DateTime.UtcNow; }

    public ModeloProcesso() { }

    public static Result<ModeloProcesso> Criar(Guid idUsuario, string nome, string? descricao)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");

        return new ModeloProcesso
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Nome = nome,
            Descricao = descricao,
            Ativo = true,
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
