using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Entities;

public class ProcessoAnexo
{
    public Guid Id { get; private set; }
    public Guid IdProcessoEtapaItem { get; private set; }
    public string NomeArquivo { get; private set; } = string.Empty;
    public string? ContentType { get; private set; }
    public long Tamanho { get; private set; }
    public string? DriveFileId { get; private set; }
    public string? Url { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAlteracao { get; private set; }
    public Guid? CriadoPor { get; private set; }
    public Guid? AlteradoPor { get; private set; }

    public void DefinirCriador(Guid ator) { CriadoPor ??= ator; }
    public void DefinirEditor(Guid ator) { AlteradoPor = ator; DataAlteracao = DateTime.UtcNow; }

    public ProcessoAnexo() { }

    public static Result<ProcessoAnexo> Criar(Guid idProcessoEtapaItem, string nomeArquivo, string? contentType, long tamanho)
    {
        if (idProcessoEtapaItem == Guid.Empty)
            return Erro.Validacao("ITEM_OBRIGATORIO", "Item do processo é obrigatório.");
        if (string.IsNullOrWhiteSpace(nomeArquivo))
            return Erro.Validacao("ARQUIVO_OBRIGATORIO", "Nome do arquivo é obrigatório.");
        if (tamanho < 0)
            return Erro.Validacao("TAMANHO_INVALIDO", "Tamanho do arquivo é inválido.");

        return new ProcessoAnexo
        {
            Id = Guid.NewGuid(),
            IdProcessoEtapaItem = idProcessoEtapaItem,
            NomeArquivo = nomeArquivo,
            ContentType = contentType,
            Tamanho = tamanho,
            DataCadastro = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow
        };
    }

    public void VincularArmazenamento(string driveFileId, string? url)
    {
        DriveFileId = driveFileId;
        Url = url;
        DataAlteracao = DateTime.UtcNow;
    }
}
