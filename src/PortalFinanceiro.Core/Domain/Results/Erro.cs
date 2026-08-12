namespace PortalFinanceiro.Core.Domain.Results;

public sealed class Erro
{
    public string Codigo { get; }
    public string Mensagem { get; }
    public ETipoErro Tipo { get; }

    private Erro(string codigo, string mensagem, ETipoErro tipo)
    {
        Codigo = codigo;
        Mensagem = mensagem;
        Tipo = tipo;
    }

    public static Erro Validacao(string codigo, string mensagem)
        => new(codigo, mensagem, ETipoErro.Validacao);

    public static Erro Negocio(string codigo, string mensagem)
        => new(codigo, mensagem, ETipoErro.Negocio);

    public static Erro NaoEncontrado(string entidade)
        => new("NAO_ENCONTRADO", $"{entidade} não encontrado.", ETipoErro.NaoEncontrado);

    public static Erro Conflito(string codigo, string mensagem)
        => new(codigo, mensagem, ETipoErro.Conflito);

    public static Erro Permissao(string codigo, string mensagem)
        => new(codigo, mensagem, ETipoErro.Permissao);

    public static Erro Externo(string codigo, string mensagem)
        => new(codigo, mensagem, ETipoErro.Externo);

    public static Erro Infraestrutura(string mensagem)
        => new("ERRO_INFRA", mensagem, ETipoErro.Infraestrutura);
}
