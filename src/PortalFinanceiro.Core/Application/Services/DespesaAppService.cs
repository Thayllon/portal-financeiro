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
    private readonly IDespesaServicoRepository _despesaServicoRepository;
    private readonly IParceriaRepository? _parceriaRepository;

    public DespesaAppService(IDespesaRepository repository, IRegraDespesaRepository regraRepository, IDespesaServicoRepository despesaServicoRepository, IParceriaRepository? parceriaRepository = null)
    {
        _repository = repository;
        _regraRepository = regraRepository;
        _despesaServicoRepository = despesaServicoRepository;
        _parceriaRepository = parceriaRepository;
    }

    public async Task<Result<IEnumerable<DespesaResponse>>> ListarAsync(Guid idUsuario, int mes, int ano, Guid? idConta = null, int? status = null, Guid? idCategoria = null, string? busca = null)
    {
        var despesas = await _repository.ListarAsync(idUsuario, mes, ano, idConta, status, idCategoria, busca);
        var responses = new List<DespesaResponse>();
        foreach (var p in despesas)
        {
            var response = Mapear(p);
            var servicos = await _despesaServicoRepository.ListarPorDespesaAsync(p.Id);
            response.Servicos = servicos.Select(s => new DespesaServicoResponse
            {
                Id = s.Id,
                CategoriaServicoId = s.CategoriaServicoId,
                SubcategoriaServicoId = s.SubcategoriaServicoId
            }).ToList();
            responses.Add(response);
        }
        return responses;
    }

    public async Task<Result<IEnumerable<DespesaResponse>>> ListarPorParceriaAsync(Guid idUsuario, Guid idParceria)
    {
        if (_parceriaRepository is null)
            return Erro.Infraestrutura("Repositório de parcerias não configurado.");

        var parceria = await _parceriaRepository.ObterPorIdAsync(idParceria);
        if (parceria is null)
            return Erro.NaoEncontrado("Parceria");
        if (parceria.IdUsuario != idUsuario)
            return Erro.Permissao("PARCERIA_ACESSO_NEGADO", "Parceria de outro usuário.");

        var despesas = await _repository.ListarPorParceriaAsync(idParceria);
        return despesas.Select(Mapear).ToList();
    }

    public async Task<Result<DespesaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var despesa = await _repository.ObterProjecaoPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");
        if (despesa.IdUsuario != idUsuario)
            return Erro.Permissao("DESPESA_ACESSO_NEGADO", "Despesa de outro usuário.");

        var response = Mapear(despesa);
        var servicos = await _despesaServicoRepository.ListarPorDespesaAsync(id);
        response.Servicos = servicos.Select(s => new DespesaServicoResponse
        {
            Id = s.Id,
            CategoriaServicoId = s.CategoriaServicoId,
            SubcategoriaServicoId = s.SubcategoriaServicoId
        }).ToList();

        return response;
    }

    public async Task<Result<DespesaResponse>> AdicionarAsync(Guid idUsuario, DespesaRequest request)
    {
        if (request.IdParceria.HasValue && _parceriaRepository is not null)
        {
            var parceria = await _parceriaRepository.ObterPorIdAsync(request.IdParceria.Value);
            if (parceria is null || parceria.IdUsuario != idUsuario || !parceria.Ativo)
                return Erro.Validacao("PARCERIA_INVALIDA", "Parceria não encontrada.");
        }

        if (!request.Repete)
        {
            var result = Despesa.Criar(idUsuario, request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria, idParceria: request.IdParceria, idCliente: request.IdCliente);
            if (!result.EhSucesso)
                return result.Erro!;

            var despesa = result.Dado!;
            await _repository.InserirAsync(despesa);

            if (request.Servicos != null && request.Servicos.Any())
            {
                var servicos = request.Servicos.Select(s => new DespesaServico(despesa.Id, s.CategoriaServicoId, s.SubcategoriaServicoId)).ToList();
                await _despesaServicoRepository.InserirEmMassaAsync(servicos);
            }

            var projecao = await _repository.ObterProjecaoPorIdAsync(despesa.Id);
            return Mapear(projecao!);
        }

        var regraResult = RegraDespesa.Criar(idUsuario, request.Descricao, request.Valor, request.Dia ?? 1, request.DiaUtil ?? false, request.IdCategoria, request.IdConta, request.Data, request.DataFim ?? request.Data);
        if (!regraResult.EhSucesso)
            return regraResult.Erro!;

        var regra = regraResult.Dado!;

        var meses = LancamentoHelper.GerarMeses(regra.DataInicio, regra.DataFim);
        var despesas = meses.Select(m => Despesa.Criar(idUsuario, regra.Descricao, regra.Valor, LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, m.Mes, m.Ano), regra.IdConta, regra.IdCategoria, null, regra.Id, idParceria: request.IdParceria, idCliente: request.IdCliente))
                            .Where(d => d.EhSucesso)
                            .Select(d => d.Dado!)
                            .ToList();

        if (despesas.Count == 0)
            return Erro.Negocio("NENHUMA_DESPESA_GERADA", "Nenhuma despesa foi gerada para o período informado.");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _regraRepository.InserirAsync(regra);
        await _repository.InserirEmMassaAsync(despesas);

        if (request.Servicos != null && request.Servicos.Any())
        {
            var servicosEmMassa = despesas.SelectMany(r => request.Servicos.Select(s => new DespesaServico(r.Id, s.CategoriaServicoId, s.SubcategoriaServicoId))).ToList();
            await _despesaServicoRepository.InserirEmMassaAsync(servicosEmMassa);
        }

        scope.Complete();

        var primeiraProjecao = await _repository.ObterProjecaoPorIdAsync(despesas.First().Id);
        return Mapear(primeiraProjecao!);
    }

    public async Task<Result<DespesaResponse>> AtualizarAsync(Guid id, Guid idUsuario, DespesaRequest request)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");
        if (despesa.IdUsuario != idUsuario)
            return Erro.Permissao("DESPESA_ACESSO_NEGADO", "Despesa de outro usuário.");

        if (request.IdParceria.HasValue && _parceriaRepository is not null)
        {
            var parceria = await _parceriaRepository.ObterPorIdAsync(request.IdParceria.Value);
            if (parceria is null || parceria.IdUsuario != despesa.IdUsuario || !parceria.Ativo)
                return Erro.Validacao("PARCERIA_INVALIDA", "Parceria não encontrada.");
        }

        var result = despesa.Atualizar(request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria, idParceria: request.IdParceria, idCliente: request.IdCliente);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(despesa);

        if (request.Servicos != null)
        {
            await _despesaServicoRepository.ExcluirPorDespesaAsync(id);
            if (request.Servicos.Any())
            {
                var servicos = request.Servicos.Select(s => new DespesaServico(id, s.CategoriaServicoId, s.SubcategoriaServicoId)).ToList();
                await _despesaServicoRepository.InserirEmMassaAsync(servicos);
            }
        }

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<DespesaResponse>> PagarAsync(Guid id, Guid idUsuario, MensalStatusRequest request)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");
        if (despesa.IdUsuario != idUsuario)
            return Erro.Permissao("DESPESA_ACESSO_NEGADO", "Despesa de outro usuário.");

        var result = despesa.Pagar(request.Data);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(despesa);

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<DespesaResponse>> EstornarAsync(Guid id, Guid idUsuario)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");
        if (despesa.IdUsuario != idUsuario)
            return Erro.Permissao("DESPESA_ACESSO_NEGADO", "Despesa de outro usuário.");

        var result = despesa.Estornar();
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(despesa);

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var despesa = await _repository.ObterPorIdAsync(id);
        if (despesa is null)
            return Erro.NaoEncontrado("Despesa");
        if (despesa.IdUsuario != idUsuario)
            return Erro.Permissao("DESPESA_ACESSO_NEGADO", "Despesa de outro usuário.");

        if (despesa.Status == Domain.Enums.StatusMensal.Realizado)
            return Erro.Negocio("DESPESA_JA_PAGA", "Não é possível excluir uma despesa já paga. Estorne primeiro.");

        despesa.Desativar();
        await _repository.AtualizarAsync(despesa);

        if (despesa.IdRegra.HasValue)
        {
            var restantes = await _repository.ContarPorRegraAsync(despesa.IdRegra.Value);
            if (restantes == 0)
            {
                var regra = await _regraRepository.ObterPorIdAsync(despesa.IdRegra.Value);
                if (regra is not null)
                {
                    regra.Desativar();
                    await _regraRepository.AtualizarAsync(regra);
                }
            }
        }

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
        IdParceria = p.IdParceria,
        Parceria = p.Parceria,
        ParceriaValor = p.ParceriaValor,
        IdCliente = p.IdCliente,
        Cliente = p.Cliente,
        DataRealizacao = p.DataRealizacao,
        IdRegra = p.IdRegra,
        EhRecorrente = p.EhRecorrente,
        IdReceitaOrigem = p.IdReceitaOrigem,
        Ativo = p.Ativo,
        DataCadastro = p.DataCadastro
    };
}
