using System.Transactions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Core.Domain.Results;
using PortalFinanceiro.Core.Domain.Services;

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

    public async Task<Result<IEnumerable<DespesaResponse>>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null)
    {
        var despesas = await _repository.ListarAsync(idUsuario, mes, ano, idConta, status, idCategoria, busca);
        return despesas.Select(Mapear).ToList();
    }

    public async Task<Result<DespesaResponse>> ObterPorIdAsync(Guid id)
    {
        var despesa = await _repository.ObterProjecaoPorIdAsync(id);
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

            var projecao = await _repository.ObterProjecaoPorIdAsync(result.Dado!.Id);
            return Mapear(projecao!);
        }

        var regraResult = RegraDespesa.Criar(idUsuario, request.Descricao, request.Valor, request.Dia ?? 1, request.DiaUtil ?? false, request.IdCategoria, request.IdConta, request.Data, request.DataFim ?? request.Data);
        if (!regraResult.EhSucesso)
            return regraResult.Erro!;

        var regra = regraResult.Dado!;

        var meses = LancamentoHelper.GerarMeses(regra.DataInicio, regra.DataFim);
        var despesas = meses.Select(m => Despesa.Criar(idUsuario, regra.Descricao, regra.Valor, LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, m.Mes, m.Ano), regra.IdConta, regra.IdCategoria, null, regra.Id))
                            .Where(d => d.EhSucesso)
                            .Select(d => d.Dado!)
                            .ToList();

        if (despesas.Count == 0)
            return Erro.Negocio("NENHUMA_DESPESA_GERADA", "Nenhuma despesa foi gerada para o período informado.");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _regraRepository.InserirAsync(regra);
        await _repository.InserirEmMassaAsync(despesas);
        scope.Complete();

        var primeiraProjecao = await _repository.ObterProjecaoPorIdAsync(despesas.First().Id);
        return Mapear(primeiraProjecao!);
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

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
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

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
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

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");

        if (despesa.Status == Domain.Enums.StatusMensal.Realizado)
            return Erro.Negocio("DESPESA_JA_PAGA", "Não é possível excluir uma despesa já paga. Estorne primeiro.");

        despesa.Desativar();
        await _repository.AtualizarAsync(despesa);
        return Resultado.Sucesso();
    }

    private static DespesaResponse Mapear(DespesaProjecao p) => new()
    {
        Id = p.Id,
        Descricao = p.Descricao,
        Valor = p.Valor,
        Data = p.Data,
        IdConta = p.IdConta,
        Conta = p.Conta,
        IdCategoria = p.IdCategoria,
        Categoria = p.Categoria,
        IdSubcategoria = p.IdSubcategoria,
        Subcategoria = p.Subcategoria,
        Status = (int)p.Status,
        DataRealizacao = p.DataRealizacao,
        IdRegra = p.IdRegra,
        EhRecorrente = p.EhRecorrente,
        IdReceitaOrigem = p.IdReceitaOrigem,
        Ativo = p.Ativo,
        DataCadastro = p.DataCadastro
    };
}
