using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class DespesaRecorrenteAppService : IDespesaRecorrenteAppService
{
    private readonly IDespesaRecorrenteRepository _repository;
    private readonly IDespesaMensalRepository _mensalRepository;

    public DespesaRecorrenteAppService(IDespesaRecorrenteRepository repository, IDespesaMensalRepository mensalRepository)
    {
        _repository = repository;
        _mensalRepository = mensalRepository;
    }

    public async Task<Result<IEnumerable<DespesaRecorrenteResponse>>> ListarAsync(Guid idUsuario)
    {
        var despesas = await _repository.ListarPorUsuarioAsync(idUsuario);
        return despesas.Select(Mapear).ToList();
    }

    public async Task<Result<DespesaRecorrenteResponse>> ObterPorIdAsync(Guid id)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa recorrente");

        return Mapear(despesa);
    }

    public async Task<Result<DespesaRecorrenteResponse>> AdicionarAsync(Guid idUsuario, DespesaRecorrenteRequest request)
    {
        var result = DespesaRecorrente.Criar(idUsuario, request.Descricao, request.Valor, request.Dia, request.IdCategoria, request.IdConta, request.DataInicio, request.DataFim);
        if (!result.EhSucesso)
            return result.Erro!;

        var despesa = result.Dado!;
        await _repository.InserirAsync(despesa);

        var meses = LancamentoHelper.GerarMeses(despesa.DataInicio, despesa.DataFim);
        var mensais = meses.Select(m => DespesaMensal.Criar(despesa.Id, m.Mes, m.Ano, despesa.Valor))
                           .Where(d => d.EhSucesso)
                           .Select(d => d.Dado!)
                           .ToList();

        if (mensais.Count != 0)
            await _mensalRepository.InserirEmMassaAsync(mensais);

        return Mapear(despesa);
    }

    public async Task<Result<DespesaRecorrenteResponse>> AtualizarAsync(Guid id, DespesaRecorrenteRequest request)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa recorrente");

        var result = despesa.Atualizar(request.Descricao, request.Valor, request.Dia, request.IdCategoria, request.IdConta, request.DataInicio, request.DataFim);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(despesa);

        var existentes = await _mensalRepository.ListarPorDespesaRecorrenteAsync(despesa.Id);
        var mesesPrevistos = LancamentoHelper.GerarMeses(despesa.DataInicio, despesa.DataFim);
        var novosMeses = mesesPrevistos
            .Where(m => !existentes.Any(e => e.Mes == m.Mes && e.Ano == m.Ano && e.Ativo))
            .ToList();

        var novosLancamentos = novosMeses
            .Select(m => DespesaMensal.Criar(despesa.Id, m.Mes, m.Ano, despesa.Valor))
            .Where(d => d.EhSucesso)
            .Select(d => d.Dado!)
            .ToList();

        if (novosLancamentos.Count != 0)
            await _mensalRepository.InserirEmMassaAsync(novosLancamentos);

        return Mapear(despesa);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa recorrente");

        despesa.Desativar();
        await _repository.AtualizarAsync(despesa);
        return Resultado.Sucesso();
    }

    private static DespesaRecorrenteResponse Mapear(DespesaRecorrente d) => new()
    {
        Id = d.Id,
        Descricao = d.Descricao,
        Valor = d.Valor,
        Dia = d.Dia,
        IdCategoria = d.IdCategoria,
        IdConta = d.IdConta,
        DataInicio = d.DataInicio,
        DataFim = d.DataFim,
        Ativo = d.Ativo,
        DataCadastro = d.DataCadastro
    };
}
