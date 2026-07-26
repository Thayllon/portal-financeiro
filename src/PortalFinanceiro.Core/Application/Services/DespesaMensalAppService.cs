using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class DespesaMensalAppService : IDespesaMensalAppService
{
    private readonly IDespesaMensalRepository _repository;
    private readonly IDespesaRecorrenteRepository _recorrenteRepository;

    public DespesaMensalAppService(IDespesaMensalRepository repository, IDespesaRecorrenteRepository recorrenteRepository)
    {
        _repository = repository;
        _recorrenteRepository = recorrenteRepository;
    }

    public async Task<Result<IEnumerable<DespesaMensalResponse>>> ListarPorMesAsync(Guid idUsuario, int mes, int ano)
    {
        var lancamentos = await _repository.ListarPorMesAsync(idUsuario, mes, ano);
        var responses = new List<DespesaMensalResponse>();

        foreach (var l in lancamentos)
        {
            var recorrente = await _recorrenteRepository.ObterPorIdAsync(l.IdDespesaRecorrente);
            responses.Add(new DespesaMensalResponse
            {
                Id = l.Id,
                IdDespesaRecorrente = l.IdDespesaRecorrente,
                Descricao = recorrente?.Descricao ?? string.Empty,
                Mes = l.Mes,
                Ano = l.Ano,
                Valor = l.Valor,
                DataPagamento = l.DataPagamento,
                Status = l.Status.ToString()
            });
        }

        return responses.OrderBy(r => r.Status == StatusMensal.Pendente.ToString() ? 0 : 1).ToList();
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

        var recorrente = await _recorrenteRepository.ObterPorIdAsync(lancamento.IdDespesaRecorrente);
        return new DespesaMensalResponse
        {
            Id = lancamento.Id,
            IdDespesaRecorrente = lancamento.IdDespesaRecorrente,
            Descricao = recorrente?.Descricao ?? string.Empty,
            Mes = lancamento.Mes,
            Ano = lancamento.Ano,
            Valor = lancamento.Valor,
            DataPagamento = lancamento.DataPagamento,
            Status = lancamento.Status.ToString()
        };
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

        var recorrente = await _recorrenteRepository.ObterPorIdAsync(lancamento.IdDespesaRecorrente);
        return new DespesaMensalResponse
        {
            Id = lancamento.Id,
            IdDespesaRecorrente = lancamento.IdDespesaRecorrente,
            Descricao = recorrente?.Descricao ?? string.Empty,
            Mes = lancamento.Mes,
            Ano = lancamento.Ano,
            Valor = lancamento.Valor,
            DataPagamento = lancamento.DataPagamento,
            Status = lancamento.Status.ToString()
        };
    }
}
