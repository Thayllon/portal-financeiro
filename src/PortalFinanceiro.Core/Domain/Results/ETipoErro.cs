namespace PortalFinanceiro.Core.Domain.Results;

public enum ETipoErro
{
    Validacao,
    Negocio,
    NaoEncontrado,
    Conflito,
    Permissao,
    Timeout,
    Externo,
    Infraestrutura
}
