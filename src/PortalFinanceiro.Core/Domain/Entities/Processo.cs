using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class Processo
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public Guid? IdParceria { get; private set; }
    public Guid? IdContrato { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }
    public Guid? CriadoPor { get; private set; }
    public Guid? AlteradoPor { get; private set; }

    public void DefinirCriador(Guid ator) { CriadoPor ??= ator; }
    public void DefinirEditor(Guid ator) { AlteradoPor = ator; DataAlteracao = DateTime.UtcNow; }

    public Processo() { }

    public static Result<Processo> Criar(Guid idUsuario, string nome, string? descricao, Guid? idParceria, Guid? idContrato)
    {
        if (idUsuario == Guid.Empty)
            return Erro.Validacao("USUARIO_OBRIGATORIO", "Usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome))
            return Erro.Validacao("NOME_OBRIGATORIO", "Nome é obrigatório.");
        var vinculo = ValidarVinculo(idParceria, idContrato);
        if (!vinculo.EhSucesso)
            return vinculo.Erro!;

        return new Processo
        {
            Id = Guid.NewGuid(),
            IdUsuario = idUsuario,
            Nome = nome,
            Descricao = descricao,
            IdParceria = idParceria,
            IdContrato = idContrato,
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

    public static Result<Unit> ValidarVinculo(Guid? idParceria, Guid? idContrato)
    {
        var temParceria = idParceria.HasValue && idParceria.Value != Guid.Empty;
        var temContrato = idContrato.HasValue && idContrato.Value != Guid.Empty;
        if (temParceria && temContrato)
            return Erro.Validacao("VINCULO_DUPLO", "Vincule o processo a uma parceria OU a um contrato, não ambos.");
        return Resultado.Sucesso();
    }
}
