using System.Text.Json.Serialization;

namespace PortalFinanceiro.Core.Domain.Results;

public class Result<T>
{
    public T? Dado { get; }
    public Erro? Erro { get; }
    public bool EhSucesso => Erro is null;

    [JsonConstructor]
    protected Result(T? dado, Erro? erro)
    {
        Dado = dado;
        Erro = erro;
    }

    public static Result<T> Sucesso(T dado) => new(dado, null);
    public static Result<T> Falha(Erro erro) => new(default, erro);

    public static implicit operator Result<T>(T dado) => Sucesso(dado);
    public static implicit operator Result<T>(Erro erro) => Falha(erro);
}
