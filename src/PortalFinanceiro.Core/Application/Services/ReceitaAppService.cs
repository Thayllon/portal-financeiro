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
    private readonly IParceriaRepository? _parceriaRepository;
    private readonly IContratoRepository? _contratoRepository;

    public ReceitaAppService(IReceitaRepository repository, IRegraReceitaRepository regraRepository, IReceitaServicoRepository receitaServicoRepository, IParceriaRepository? parceriaRepository = null, IContratoRepository? contratoRepository = null)
    {
        _repository = repository;
        _regraRepository = regraRepository;
        _receitaServicoRepository = receitaServicoRepository;
        _parceriaRepository = parceriaRepository;
        _contratoRepository = contratoRepository;
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
                CategoriaServico = s.CategoriaServico,
                SubcategoriaServicoId = s.SubcategoriaServicoId,
                SubcategoriaServico = s.SubcategoriaServico
            }).ToList();
            responses.Add(response);
        }
        return responses;
    }

    public async Task<Result<IEnumerable<ReceitaResponse>>> ListarPorParceriaAsync(Guid idUsuario, Guid idParceria)
    {
        if (_parceriaRepository is null)
            return Erro.Infraestrutura("Repositório de parcerias não configurado.");

        var parceria = await _parceriaRepository.ObterPorIdAsync(idParceria);
        if (parceria is null)
            return Erro.NaoEncontrado("Parceria");
        if (parceria.IdUsuario != idUsuario)
            return Erro.Permissao("PARCERIA_ACESSO_NEGADO", "Parceria de outro usuário.");

        var receitas = await _repository.ListarPorParceriaAsync(idParceria);
        return receitas.Select(Mapear).ToList();
    }

    public async Task<Result<IEnumerable<ReceitaResponse>>> ListarPorContratoAsync(Guid idUsuario, Guid idContrato)
    {
        if (_contratoRepository is null)
            return Erro.Infraestrutura("Repositório de contratos não configurado.");

        var contrato = await _contratoRepository.ObterPorIdAsync(idContrato);
        if (contrato is null)
            return Erro.NaoEncontrado("Contrato");
        if (contrato.IdUsuario != idUsuario)
            return Erro.Permissao("CONTRATO_ACESSO_NEGADO", "Contrato de outro usuário.");

        var receitas = await _repository.ListarPorContratoAsync(idContrato);
        return receitas.Select(Mapear).ToList();
    }

    public async Task<Result<ReceitaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var receita = await _repository.ObterProjecaoPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");
        if (receita.IdUsuario != idUsuario)
            return Erro.Permissao("RECEITA_ACESSO_NEGADO", "Receita de outro usuário.");

        var response = Mapear(receita);
        var servicos = await _receitaServicoRepository.ListarPorReceitaAsync(id);
        response.Servicos = servicos.Select(s => new ReceitaServicoResponse
        {
            Id = s.Id,
            CategoriaServicoId = s.CategoriaServicoId,
            CategoriaServico = s.CategoriaServico,
            SubcategoriaServicoId = s.SubcategoriaServicoId,
            SubcategoriaServico = s.SubcategoriaServico
        }).ToList();

        return response;
    }

    public async Task<Result<ReceitaResponse>> AdicionarAsync(Guid idUsuario, ReceitaRequest request)
    {
        if (request.IdParceria.HasValue && request.IdContrato.HasValue)
            return Erro.Validacao("RECEITA_VINCULO_DUPLO", "Informe parceria ou contrato, nunca os dois.");

        if (request.IdParceria.HasValue && _parceriaRepository is not null)
        {
            var parceria = await _parceriaRepository.ObterPorIdAsync(request.IdParceria.Value);
            if (parceria is null || parceria.IdUsuario != idUsuario || !parceria.Ativo)
                return Erro.Validacao("PARCERIA_INVALIDA", "Parceria não encontrada.");
        }

        if (request.IdContrato.HasValue)
        {
            if (_contratoRepository is null)
                return Erro.Infraestrutura("Repositório de contratos não configurado.");
            var contrato = await _contratoRepository.ObterPorIdAsync(request.IdContrato.Value);
            if (contrato is null || contrato.IdUsuario != idUsuario || !contrato.Ativo)
                return Erro.Validacao("CONTRATO_INVALIDO", "Contrato não encontrado.");
        }

        if (!request.Repete)
        {
            var result = Receita.Criar(idUsuario, request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria,
                idParceiro: request.IdParceiro, idCliente: request.IdCliente, idParceria: request.IdParceria, idContrato: request.IdContrato);
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
        var receitas = new List<Receita>();
        foreach (var m in meses)
        {
            var criada = Receita.Criar(idUsuario, regra.Descricao, regra.Valor, LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, m.Mes, m.Ano), regra.IdConta, regra.IdCategoria, null, regra.Id,
                idParceiro: request.IdParceiro, idCliente: request.IdCliente, idParceria: request.IdParceria, idContrato: request.IdContrato);
            if (!criada.EhSucesso)
                return criada.Erro!;

            receitas.Add(criada.Dado!);
        }

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

    public async Task<Result<ReceitaResponse>> AtualizarAsync(Guid id, Guid idUsuario, ReceitaRequest request)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");
        if (receita.IdUsuario != idUsuario)
            return Erro.Permissao("RECEITA_ACESSO_NEGADO", "Receita de outro usuário.");

        if (request.IdParceria.HasValue && request.IdContrato.HasValue)
            return Erro.Validacao("RECEITA_VINCULO_DUPLO", "Informe parceria ou contrato, nunca os dois.");

        if (request.IdParceria.HasValue && _parceriaRepository is not null)
        {
            var parceria = await _parceriaRepository.ObterPorIdAsync(request.IdParceria.Value);
            if (parceria is null || parceria.IdUsuario != receita.IdUsuario || !parceria.Ativo)
                return Erro.Validacao("PARCERIA_INVALIDA", "Parceria não encontrada.");
        }

        if (request.IdContrato.HasValue)
        {
            if (_contratoRepository is null)
                return Erro.Infraestrutura("Repositório de contratos não configurado.");
            var contrato = await _contratoRepository.ObterPorIdAsync(request.IdContrato.Value);
            if (contrato is null || contrato.IdUsuario != receita.IdUsuario || !contrato.Ativo)
                return Erro.Validacao("CONTRATO_INVALIDO", "Contrato não encontrado.");
        }

        var result = receita.Atualizar(request.Descricao, request.Valor, request.Data, request.IdConta, request.IdCategoria, request.IdSubcategoria,
            idParceiro: request.IdParceiro, idCliente: request.IdCliente, idParceria: request.IdParceria, idContrato: request.IdContrato);
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

    public async Task<Result<ReceitaResponse>> ReceberAsync(Guid id, Guid idUsuario, MensalStatusRequest request)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");
        if (receita.IdUsuario != idUsuario)
            return Erro.Permissao("RECEITA_ACESSO_NEGADO", "Receita de outro usuário.");

        var result = receita.Receber(request.Data);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(receita);

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<ReceitaResponse>> EstornarAsync(Guid id, Guid idUsuario)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");
        if (receita.IdUsuario != idUsuario)
            return Erro.Permissao("RECEITA_ACESSO_NEGADO", "Receita de outro usuário.");

        var result = receita.Estornar();
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(receita);

        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var receita = await _repository.ObterPorIdAsync(id);
        if (receita is null)
            return Erro.NaoEncontrado("Receita");
        if (receita.IdUsuario != idUsuario)
            return Erro.Permissao("RECEITA_ACESSO_NEGADO", "Receita de outro usuário.");

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
        IdParceria = p.IdParceria,
        Parceria = p.Parceria,
        ParceriaValor = p.ParceriaValor,
        ParceriaPercentual = p.ParceriaPercentual,
        IdContrato = p.IdContrato,
        Contrato = p.Contrato,
        Status = (int)p.Status,
        DataRealizacao = p.DataRealizacao,
        IdRegra = p.IdRegra,
        EhRecorrente = p.EhRecorrente,
        Ativo = p.Ativo,
        DataCadastro = p.DataCadastro
    };
}
