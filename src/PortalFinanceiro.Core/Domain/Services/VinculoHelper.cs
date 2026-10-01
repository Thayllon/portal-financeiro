using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Domain.Services;

public static class VinculoHelper
{
    public static async Task<Result<Unit>> ValidarAtualizacaoAsync<T>(
        Guid? idVinculo,
        Guid? idVinculoAtual,
        Guid idUsuario,
        Func<Guid, Task<T?>> obterPorId,
        Func<T, Guid> obterIdUsuario,
        Func<T, bool> obterAtivo,
        string codigoErro,
        string mensagemErro) where T : class
    {
        if (!idVinculo.HasValue)
            return Resultado.Sucesso();

        var vinculo = await obterPorId(idVinculo.Value);
        if (vinculo is null || obterIdUsuario(vinculo) != idUsuario)
            return Erro.Validacao(codigoErro, mensagemErro);

        if (idVinculo == idVinculoAtual)
            return Resultado.Sucesso();

        return obterAtivo(vinculo)
            ? Resultado.Sucesso()
            : Erro.Validacao(codigoErro, mensagemErro);
    }
}