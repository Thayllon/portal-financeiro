using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class ReceitaMensalAppService : IReceitaMensalAppService
{
    private readonly IReceitaMensalRepository _repository;

    public ReceitaMensalAppService(IReceitaMensalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ReceitaMensalResponse>>> ListarPorMesAsync(Guid idUsuario, int mes, int ano)
    {
        var lancamentos = await _repository.ListarPorMesAsync(idUsuario, mes, ano);
        return lancamentos.Select(Mapear).ToList();
    }

    public async Task<Result<ReceitaMensalResponse>> ReceberAsync(Guid id, MensalStatusRequest request)
    {
        var lancamento = await _repository.ObterPorIdAsync(id);
        if (lancamento is null)
            return Erro.NaoEncontrado("Receita mensal");

        var result = lancamento.Receber(request.Data);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(lancamento);
        return Mapear(lancamento);
    }

    public async Task<Result<ReceitaMensalResponse>> EstornarAsync(Guid id)
    {
        var lancamento = await _repository.ObterPorIdAsync(id);
        if (lancamento is null)
            return Erro.NaoEncontrado("Receita mensal");

        var result = lancamento.Estornar();
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(lancamento);
        return Mapear(lancamento);
    }

    private static ReceitaMensalResponse Mapear(ReceitaMensal l) => new()
    {
        Id = l.Id,
        IdReceitaRecorrente = l.IdReceitaRecorrente,
        Descricao = l.Descricao,
        Mes = l.Mes,
        Ano = l.Ano,
        Valor = l.Valor,
        DataRecebimento = l.DataRecebimento,
        Status = l.Status.ToString()
    };
}
