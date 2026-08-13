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

public class ReceitaAppService : IReceitaAppService
{
    private readonly IReceitaRepository _repository;
    private readonly IRegraReceitaRepository _regraRepository;

    public ReceitaAppService(IReceitaRepository repository, IRegraReceitaRepository regraRepository)
    {
        _repository = repository;
        _regraRepository = regraRepository;
    }

    public async Task<Result<IEnumerable<ReceitaResponse>>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null)
    {
        var receitas = await _repository.ListarAsync(idUsuario, mes, ano, idConta, status, idCategoria, busca);
        return receitas.Select(Mapear).ToList();
    }

    public async Task<Result<ReceitaResponse>> ObterPorIdAsync(Guid id)
    {
        var receita = await _repository.ObterProjecaoPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");

        return Mapear(receita);
    }

    public async Task<Result<ReceitaResponse>> AdicionarAsync(Guid idUsuario, ReceitaRequest request)
    {
        if (!request.Repete)
        {
            var result = Receita.Criar(idUsuario, request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria);
            if (!result.EhSucesso)
                return result.Erro!;

            var receita = result.Dado!;
            await _repository.InserirAsync(receita);

            var projecao = await _repository.ObterProjecaoPorIdAsync(receita.Id);
            return Mapear(projecao!);
        }

        var regraResult = RegraReceita.Criar(idUsuario, request.Descricao, request.Valor, request.Dia ?? 1, request.DiaUtil ?? false, request.IdCategoria, request.IdConta, request.Data, request.DataFim ?? request.Data);
        if (!regraResult.EhSucesso)
            return regraResult.Erro!;

        var regra = regraResult.Dado!;

        var meses = LancamentoHelper.GerarMeses(regra.DataInicio, regra.DataFim);
        var receitas = meses.Select(m => Receita.Criar(idUsuario, regra.Descricao, regra.Valor, LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, m.Mes, m.Ano), regra.IdConta, regra.IdCategoria, null, regra.Id))
                            .Where(r => r.EhSucesso)
                            .Select(r => r.Dado!)
                            .ToList();

        if (receitas.Count == 0)
            return Erro.Negocio("NENHUMA_RECEITA_GERADA", "Nenhuma receita foi gerada para o período informado.");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _regraRepository.InserirAsync(regra);
        await _repository.InserirEmMassaAsync(receitas);
        scope.Complete();

        var primeiraProjecao = await _repository.ObterProjecaoPorIdAsync(receitas.First().Id);
        return Mapear(primeiraProjecao!);
    }

    public async Task<Result<ReceitaResponse>> AtualizarAsync(Guid id, ReceitaRequest request)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");

        var result = receita.Atualizar(request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(receita);

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<ReceitaResponse>> ReceberAsync(Guid id, MensalStatusRequest request)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");

        var result = receita.Receber(request.Data);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(receita);

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<ReceitaResponse>> EstornarAsync(Guid id)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");

        var result = receita.Estornar();
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(receita);

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");

        if (receita.Status == Domain.Enums.StatusMensal.Realizado)
            return Erro.Negocio("RECEITA_JA_RECEBIDA", "Não é possível excluir uma receita já recebida. Estorne primeiro.");

        receita.Desativar();
        await _repository.AtualizarAsync(receita);
        return Resultado.Sucesso();
    }

    private static ReceitaResponse Mapear(ReceitaProjecao p) => new()
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
        Ativo = p.Ativo,
        DataCadastro = p.DataCadastro
    };
}
