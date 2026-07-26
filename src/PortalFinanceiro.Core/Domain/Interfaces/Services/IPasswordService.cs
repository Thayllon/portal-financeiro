namespace PortalFinanceiro.Core.Domain.Interfaces.Services;

public interface IPasswordService
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}
