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
    private readonly IReceitaServicoRepository _receitaServicoRepository;

    public ReceitaAppService(IReceitaRepository repository, IRegraReceitaRepository regraRepository, IReceitaServicoRepository receitaServicoRepository)
    {
        _repository = repository;
        _regraRepository = regraRepository;
        _receitaServicoRepository = receitaServicoRepository;
    }

    public async Task<Result<IEnumerable<ReceitaResponse>>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null)
    {
        var receitas = await _repository.ListarAsync(idUsuario, mes, ano, idConta, status, idCategoria, busca);
        var responses = new List<ReceitaResponse>();
        foreach (var p in receitas)
        {
            var response = Mapear(p);
            var servicos = await _receitaServicoRepository.ListarPorReceitaAsync(p.Id);
            response.Servicos = servicos.Select(s => new ReceitaServicoResponse
            {
                Id = s.Id,
                CategoriaServicoId = s.CategoriaServicoId,
                SubcategoriaServicoId = s.SubcategoriaServicoId
            }).ToList();
            responses.Add(response);
        }
        return responses;
    }

    public async Task<Result<ReceitaResponse>> ObterPorIdAsync(Guid id)
    {
        var receita = await _repository.ObterProjecaoPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");

        var response = Mapear(receita);
        var servicos = await _receitaServicoRepository.ListarPorReceitaAsync(id);
        response.Servicos = servicos.Select(s => new ReceitaServicoResponse
        {
            Id = s.Id,
            CategoriaServicoId = s.CategoriaServicoId,
            SubcategoriaServicoId = s.SubcategoriaServicoId
        }).ToList();

        return response;
    }

    public async Task<Result<ReceitaResponse>> AdicionarAsync(Guid idUsuario, ReceitaRequest request)
    {
        if (!request.Repete)
        {
            var result = Receita.Criar(idUsuario, request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria,
                idParceiro: request.IdParceiro, idCliente: request.IdCliente);
            if (!result.EhSucesso)
                return result.Erro!;

            var receita = result.Dado!;
            await _repository.InserirAsync(receita);

            if (request.Servicos != null && request.Servicos.Any())
            {
                var servicos = request.Servicos.Select(s => new ReceitaServico(receita.Id, s.CategoriaServicoId, s.SubcategoriaServicoId)).ToList();
                await _receitaServicoRepository.InserirEmMassaAsync(servicos);
            }

            var projecao = await _repository.ObterProjecaoPorIdAsync(receita.Id);
            return Mapear(projecao!);
        }

        var regraResult = RegraReceita.Criar(idUsuario, request.Descricao, request.Valor, request.Dia ?? 1, request.DiaUtil ?? false, request.IdCategoria, request.IdConta, request.Data, request.DataFim ?? request.Data);
        if (!regraResult.EhSucesso)
            return regraResult.Erro!;

        var regra = regraResult.Dado!;

        var meses = LancamentoHelper.GerarMeses(regra.DataInicio, regra.DataFim);
        var receitas = meses.Select(m => Receita.Criar(idUsuario, regra.Descricao, regra.Valor, LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, m.Mes, m.Ano), regra.IdConta, regra.IdCategoria, null, regra.Id,
                                idParceiro: request.IdParceiro, idCliente: request.IdCliente))
                            .Where(r => r.EhSucesso)
                            .Select(r => r.Dado!)
                            .ToList();

        if (receitas.Count == 0)
            return Erro.Negocio("NENHUMA_RECEITA_GERADA", "Nenhuma receita foi gerada para o período informado.");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _regraRepository.InserirAsync(regra);
        await _repository.InserirEmMassaAsync(receitas);

        if (request.Servicos != null && request.Servicos.Any())
        {
            var servicosEmMassa = receitas.SelectMany(r => request.Servicos.Select(s => new ReceitaServico(r.Id, s.CategoriaServicoId, s.SubcategoriaServicoId))).ToList();
            await _receitaServicoRepository.InserirEmMassaAsync(servicosEmMassa);
        }

        scope.Complete();

        var primeiraProjecao = await _repository.ObterProjecaoPorIdAsync(receitas.First().Id);
        return Mapear(primeiraProjecao!);
    }

    public async Task<Result<ReceitaResponse>> AtualizarAsync(Guid id, ReceitaRequest request)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");

        var result = receita.Atualizar(request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria,
            idParceiro: request.IdParceiro, idCliente: request.IdCliente);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(receita);

        if (request.Servicos != null)
        {
            await _receitaServicoRepository.ExcluirPorReceitaAsync(id);
            if (request.Servicos.Any())
            {
                var servicos = request.Servicos.Select(s => new ReceitaServico(id, s.CategoriaServicoId, s.SubcategoriaServicoId)).ToList();
                await _receitaServicoRepository.InserirEmMassaAsync(servicos);
            }
        }

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

        if (receita.IdRegra.HasValue)
        {
            var restantes = await _repository.ContarPorRegraAsync(receita.IdRegra.Value);
            if (restantes == 0)
            {
                var regra = await _regraRepository.ObterPorIdAsync(receita.IdRegra.Value);
                if (regra is not null)
                {
                    regra.Desativar();
                    await _regraRepository.AtualizarAsync(regra);
                }
            }
        }

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
        IdParceiro = p.IdParceiro,
        Parceiro = p.Parceiro,
        IdCliente = p.IdCliente,
        Cliente = p.Cliente,
        Status = (int)p.Status,
        DataRealizacao = p.DataRealizacao,
        IdRegra = p.IdRegra,
        EhRecorrente = p.EhRecorrente,
        Ativo = p.Ativo,
        DataCadastro = p.DataCadastro
    };
}
