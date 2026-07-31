using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class DespesaMensalAppService : IDespesaMensalAppService
{
    private readonly IDespesaMensalRepository _repository;

    public DespesaMensalAppService(IDespesaMensalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<DespesaMensalResponse>>> ListarPorMesAsync(Guid idUsuario, int mes, int ano)
    {
        var lancamentos = await _repository.ListarPorMesAsync(idUsuario, mes, ano);
        return lancamentos.Select(Mapear).ToList();
    }

    public async Task<Result<DespesaMensalResponse>> PagarAsync(Guid id, MensalStatusRequest request)
    {
        var lancamento = await _repository.ObterPorIdAsync(id);
        if (lancamento is null)
            return Erro.NaoEncontrado("Despesa mensal");

        var result = lancamento.Pagar(request.Data);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(lancamento);
        return Mapear(lancamento);
    }

    public async Task<Result<DespesaMensalResponse>> EstornarAsync(Guid id)
    {
        var lancamento = await _repository.ObterPorIdAsync(id);
        if (lancamento is null)
            return Erro.NaoEncontrado("Despesa mensal");

        var result = lancamento.Estornar();
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(lancamento);
        return Mapear(lancamento);
    }

    private static DespesaMensalResponse Mapear(DespesaMensal l) => new()
    {
        Id = l.Id,
        IdDespesaRecorrente = l.IdDespesaRecorrente,
        Descricao = l.Descricao,
        Mes = l.Mes,
        Ano = l.Ano,
        Valor = l.Valor,
        DataPagamento = l.DataPagamento,
        Status = l.Status.ToString()
    };
}
