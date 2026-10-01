using Microsoft.Extensions.Logging;

namespace PortalFinanceiro.Core.Application.Services;

/// <summary>
/// Rastreio de ações de negócio: quem (ator) fez o quê (ação) em qual entidade.
/// Eventos estruturados consumidos pelo Serilog (console + arquivo).
/// </summary>
internal static class AuditoriaLogger
{
    public static void Auditar(this ILogger logger, Guid ator, string acao, string entidade, Guid entidadeId)
        => logger.LogInformation("Auditoria {Ator} {Acao} {Entidade} {EntidadeId}", ator, acao, entidade, entidadeId);

    public static void AuditarNegado(this ILogger logger, Guid? ator, string motivo, string entidade, Guid? entidadeId = null)
        => logger.LogWarning("AuditoriaNegada {Ator} {Motivo} {Entidade} {EntidadeId}", ator, motivo, entidade, entidadeId);
}
