using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class DespesaAppService : IDespesaAppService
{
    private readonly IDespesaRepository _repository;
    private readonly IRegraDespesaRepository _regraRepository;

    public DespesaAppService(IDespesaRepository repository, IRegraDespesaRepository regraRepository)
    {
        _repository = repository;
        _regraRepository = regraRepository;
    }

    public async Task<Result<IEnumerable<DespesaResponse>>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, string? status = null, Guid? idCategoria = null, string? busca = null)
    {
        var despesas = await _repository.ListarAsync(idUsuario, mes, ano, idConta, status, idCategoria, busca);
        return despesas.Select(Mapear).ToList();
    }

    public async Task<Result<DespesaResponse>> ObterPorIdAsync(Guid id)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");

        return Mapear(despesa);
    }

    public async Task<Result<DespesaResponse>> AdicionarAsync(Guid idUsuario, DespesaRequest request)
    {
        if (!request.Repete)
        {
            var result = Despesa.Criar(idUsuario, request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria);
            if (!result.EhSucesso)
                return result.Erro!;

            await _repository.InserirAsync(result.Dado!);
            return Mapear(result.Dado!);
        }

        var regraResult = RegraDespesa.Criar(idUsuario, request.Descricao, request.Valor, request.Dia ?? 1, request.DiaUtil ?? false, request.IdCategoria, request.IdConta, request.Data, request.DataFim ?? request.Data);
        if (!regraResult.EhSucesso)
            return regraResult.Erro!;

        var regra = regraResult.Dado!;
        await _regraRepository.InserirAsync(regra);

        var meses = LancamentoHelper.GerarMeses(regra.DataInicio, regra.DataFim);
        var despesas = meses.Select(m => Despesa.Criar(idUsuario, regra.Descricao, regra.Valor, LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, m.Mes, m.Ano), regra.IdConta, regra.IdCategoria, null, regra.Id))
                            .Where(d => d.EhSucesso)
                            .Select(d => d.Dado!)
                            .ToList();

        if (despesas.Count != 0)
            await _repository.InserirEmMassaAsync(despesas);

        return Mapear(despesas.First());
    }

    public async Task<Result<DespesaResponse>> AtualizarAsync(Guid id, DespesaRequest request)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");

        var result = despesa.Atualizar(request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(despesa);
        return Mapear(despesa);
    }

    public async Task<Result<DespesaResponse>> PagarAsync(Guid id, MensalStatusRequest request)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");

        var result = despesa.Pagar(request.Data);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(despesa);
        return Mapear(despesa);
    }

    public async Task<Result<DespesaResponse>> EstornarAsync(Guid id)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");

        var result = despesa.Estornar();
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(despesa);
        return Mapear(despesa);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");

        await _repository.ExcluirAsync(id);
        return Resultado.Sucesso();
    }

    private static DespesaResponse Mapear(Despesa d) => new()
    {
        Id = d.Id,
        Descricao = d.Descricao,
        Valor = d.Valor,
        Data = d.Data,
        IdConta = d.IdConta,
        Conta = d.Conta,
        IdCategoria = d.IdCategoria,
        Categoria = d.Categoria,
        IdSubcategoria = d.IdSubcategoria,
        Subcategoria = d.Subcategoria,
        Status = d.Status.ToString(),
        DataRealizacao = d.DataRealizacao,
        IdRegra = d.IdRegra,
        EhRecorrente = d.EhRecorrente,
        Ativo = d.Ativo,
        DataCadastro = d.DataCadastro
    };
}
