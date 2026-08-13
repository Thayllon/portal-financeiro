using FluentAssertions;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

[Trait("Categoria", "Dominio")]
public class RegraReceitaTests
{
    private readonly Guid _idUsuario = Guid.NewGuid();
    private readonly Guid _idCategoria = Guid.NewGuid();
    private readonly Guid _idConta = Guid.NewGuid();
    private readonly DateTime _dataInicio = new(2025, 1, 1);
    private readonly DateTime _dataFim = new(2025, 12, 31);

    public record CriarParams(Guid IdUsuario, string Descricao, decimal Valor, int Dia, bool DiaUtil, Guid IdCategoria, Guid IdConta, DateTime DataInicio, DateTime DataFim);

    public static readonly TheoryData<CriarParams, string> ObterDadosCriarValidacao = new()
    {
        { new(Guid.Empty, "Salário", 5000m, 5, false, Guid.NewGuid(), Guid.NewGuid(), new(2025,1,1), new(2025,12,31)), "USUARIO_OBRIGATORIO" },
        { new(Guid.NewGuid(), "", 5000m, 5, false, Guid.NewGuid(), Guid.NewGuid(), new(2025,1,1), new(2025,12,31)), "DESCRICAO_OBRIGATORIA" },
        { new(Guid.NewGuid(), "Salário", 0, 5, false, Guid.NewGuid(), Guid.NewGuid(), new(2025,1,1), new(2025,12,31)), "VALOR_INVALIDO" },
        { new(Guid.NewGuid(), "Salário", 5000m, 6, true, Guid.NewGuid(), Guid.NewGuid(), new(2025,1,1), new(2025,12,31)), "DIA_UTIL_INVALIDO" },
        { new(Guid.NewGuid(), "Salário", 5000m, 32, false, Guid.NewGuid(), Guid.NewGuid(), new(2025,1,1), new(2025,12,31)), "DIA_INVALIDO" },
        { new(Guid.NewGuid(), "Salário", 5000m, 5, false, Guid.Empty, Guid.NewGuid(), new(2025,1,1), new(2025,12,31)), "CATEGORIA_OBRIGATORIA" },
        { new(Guid.NewGuid(), "Salário", 5000m, 5, false, Guid.NewGuid(), Guid.Empty, new(2025,1,1), new(2025,12,31)), "CONTA_OBRIGATORIA" },
        { new(Guid.NewGuid(), "Salário", 5000m, 5, false, Guid.NewGuid(), Guid.NewGuid(), new(2025,12,31), new(2025,1,1)), "PERIODO_INVALIDO" },
    };

    [Theory]
    [MemberData(nameof(ObterDadosCriarValidacao))]
    public void Criar_DadosInvalidos_RetornaValidacao(CriarParams p, string codigoErro)
    {
        var result = RegraReceita.Criar(p.IdUsuario, p.Descricao, p.Valor, p.Dia, p.DiaUtil, p.IdCategoria, p.IdConta, p.DataInicio, p.DataFim);
        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be(codigoErro);
    }

    [Fact]
    public void Criar_ComDadosValidos_RetornaSucesso()
    {
        var result = RegraReceita.Criar(_idUsuario, "Salário", 5000m, 5, false, _idCategoria, _idConta, _dataInicio, _dataFim);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.IdUsuario.Should().Be(_idUsuario);
        result.Dado.Descricao.Should().Be("Salário");
        result.Dado.Valor.Should().Be(5000m);
        result.Dado.Dia.Should().Be(5);
        result.Dado.DiaUtil.Should().BeFalse();
        result.Dado.IdCategoria.Should().Be(_idCategoria);
        result.Dado.IdConta.Should().Be(_idConta);
        result.Dado.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Atualizar_AlteraDados()
    {
        var criar = RegraReceita.Criar(_idUsuario, "Salário", 5000m, 5, false, _idCategoria, _idConta, _dataInicio, _dataFim);
        var regra = criar.Dado!;

        var result = regra.Atualizar("Freela", 3000m, 3, true, _idCategoria, _idConta, _dataInicio, _dataFim);

        result.EhSucesso.Should().BeTrue();
        regra.Descricao.Should().Be("Freela");
        regra.Valor.Should().Be(3000m);
        regra.Dia.Should().Be(3);
        regra.DiaUtil.Should().BeTrue();
    }

    [Fact]
    public void Desativar_AlteraAtivo()
    {
        var criar = RegraReceita.Criar(_idUsuario, "Salário", 5000m, 5, false, _idCategoria, _idConta, _dataInicio, _dataFim);
        var regra = criar.Dado!;

        regra.Desativar();

        regra.Ativo.Should().BeFalse();
    }
}
