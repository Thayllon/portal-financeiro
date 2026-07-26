namespace PortalFinanceiro.Core.Domain.Results;

public enum ETipoErro
{
    Validacao,
    Negocio,
    NaoEncontrado,
    Conflito,
    Timeout,
    Externo,
    Infraestrutura
}
