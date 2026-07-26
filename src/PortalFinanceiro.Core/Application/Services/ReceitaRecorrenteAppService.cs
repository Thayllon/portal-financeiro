using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class ReceitaRecorrenteAppService : IReceitaRecorrenteAppService
{
    private readonly IReceitaRecorrenteRepository _repository;
    private readonly IReceitaMensalRepository _mensalRepository;

    public ReceitaRecorrenteAppService(IReceitaRecorrenteRepository repository, IReceitaMensalRepository mensalRepository)
    {
        _repository = repository;
        _mensalRepository = mensalRepository;
    }

    public async Task<Result<IEnumerable<ReceitaRecorrenteResponse>>> ListarAsync(Guid idUsuario)
    {
        var receitas = await _repository.ListarPorUsuarioAsync(idUsuario);
        return receitas.Select(Mapear).ToList();
    }

    public async Task<Result<ReceitaRecorrenteResponse>> ObterPorIdAsync(Guid id)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita recorrente");

        return Mapear(receita);
    }

    public async Task<Result<ReceitaRecorrenteResponse>> AdicionarAsync(Guid idUsuario, ReceitaRecorrenteRequest request)
    {
        var result = ReceitaRecorrente.Criar(idUsuario, request.Descricao, request.Valor, request.Dia, request.IdCategoria, request.IdConta, request.DataInicio, request.DataFim);
        if (!result.EhSucesso)
            return result.Erro!;

        var receita = result.Dado!;
        await _repository.InserirAsync(receita);

        var meses = LancamentoHelper.GerarMeses(receita.DataInicio, receita.DataFim);
        var mensais = meses.Select(m => ReceitaMensal.Criar(receita.Id, m.Mes, m.Ano, receita.Valor))
                           .Where(r => r.EhSucesso)
                           .Select(r => r.Dado!)
                           .ToList();

        if (mensais.Count != 0)
            await _mensalRepository.InserirEmMassaAsync(mensais);

        return Mapear(receita);
    }

    public async Task<Result<ReceitaRecorrenteResponse>> AtualizarAsync(Guid id, ReceitaRecorrenteRequest request)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita recorrente");

        var result = receita.Atualizar(request.Descricao, request.Valor, request.Dia, request.IdCategoria, request.IdConta, request.DataInicio, request.DataFim);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(receita);

        var existentes = await _mensalRepository.ListarPorReceitaRecorrenteAsync(receita.Id);
        var mesesPrevistos = LancamentoHelper.GerarMeses(receita.DataInicio, receita.DataFim);
        var novosMeses = mesesPrevistos
            .Where(m => !existentes.Any(e => e.Mes == m.Mes && e.Ano == m.Ano && e.Ativo))
            .ToList();

        var novosLancamentos = novosMeses
            .Select(m => ReceitaMensal.Criar(receita.Id, m.Mes, m.Ano, receita.Valor))
            .Where(r => r.EhSucesso)
            .Select(r => r.Dado!)
            .ToList();

        if (novosLancamentos.Count != 0)
            await _mensalRepository.InserirEmMassaAsync(novosLancamentos);

        return Mapear(receita);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita recorrente");

        receita.Desativar();
        await _repository.AtualizarAsync(receita);
        return Resultado.Sucesso();
    }

    private static ReceitaRecorrenteResponse Mapear(ReceitaRecorrente r) => new()
    {
        Id = r.Id,
        Descricao = r.Descricao,
        Valor = r.Valor,
        Dia = r.Dia,
        IdCategoria = r.IdCategoria,
        IdConta = r.IdConta,
        DataInicio = r.DataInicio,
        DataFim = r.DataFim,
        Ativo = r.Ativo,
        DataCadastro = r.DataCadastro
    };
}
