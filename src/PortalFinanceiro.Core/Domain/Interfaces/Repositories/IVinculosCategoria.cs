namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IVinculosCategoria
{
    Task<bool> PossuiVinculosAsync(Guid categoriaId, string tabelaVinculo);
}
