using System.Transactions;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Core.Domain.Results;
using PortalFinanceiro.Core.Domain.Services;

namespace PortalFinanceiro.Core.Application.Services;

public class ContratoAppService : IContratoAppService
{
    private readonly IContratoRepository _repository;
    private readonly IPessoaRepository _pessoaRepository;
    private readonly IRegraReceitaRepository? _regraRepository;
    private readonly IReceitaRepository? _receitaRepository;
    private readonly ICategoriaReceitaRepository? _categoriaRepository;
    private readonly IContaBancariaRepository? _contaRepository;

    public ContratoAppService(
        IContratoRepository repository,
        IPessoaRepository pessoaRepository,
        IRegraReceitaRepository? regraRepository = null,
        IReceitaRepository? receitaRepository = null,
        ICategoriaReceitaRepository? categoriaRepository = null,
        IContaBancariaRepository? contaRepository = null)
    {
        _repository = repository;
        _pessoaRepository = pessoaRepository;
        _regraRepository = regraRepository;
        _receitaRepository = receitaRepository;
        _categoriaRepository = categoriaRepository;
        _contaRepository = contaRepository;
    }

    public async Task<Result<IEnumerable<ContratoResponse>>> ListarAsync(Guid idUsuario, bool? ativo = null, bool? ehRecorrente = null)
    {
        var contratos = await _repository.ListarComTotaisAsync(idUsuario, ativo, ehRecorrente, (int)StatusMensal.Realizado);
        var responses = new List<ContratoResponse>();
        foreach (var c in contratos)
            responses.Add(await MapearComResumoAsync(c, c.TotalRecebido));
        return responses;
    }

    public async Task<Result<ContratoResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var contrato = await _repository.ObterProjecaoPorIdAsync(id);
        if (contrato is null)
            return Erro.NaoEncontrado("Contrato");
        if (contrato.IdUsuario != idUsuario)
            return Erro.Permissao("CONTRATO_ACESSO_NEGADO", "Contrato de outro usuário.");
        return await MapearComResumoAsync(contrato);
    }

    public async Task<Result<ContratoResponse>> AdicionarAsync(Guid idUsuario, ContratoRequest request)
    {
        var validacao = await ValidarClienteAsync(idUsuario, request.IdCliente);
        if (!validacao.EhSucesso)
            return validacao.Erro!;

        if (!request.EhRecorrente)
        {
            var result = Contrato.Criar(idUsuario, request.Nome, request.IdCliente, request.Valor);
            if (!result.EhSucesso)
                return result.Erro!;

            await _repository.InserirAsync(result.Dado!);
            var projecao = await _repository.ObterProjecaoPorIdAsync(result.Dado!.Id);
            return await MapearComResumoAsync(projecao!);
        }

        if (_regraRepository is null || _receitaRepository is null)
            return Erro.Infraestrutura("Repositórios de recorrência não configurados.");

        var validRec = await ValidarRecorrenciaAsync(idUsuario, request);
        if (!validRec.EhSucesso)
            return validRec.Erro!;

        var contratoResult = Contrato.Criar(idUsuario, request.Nome, request.IdCliente, request.Valor, ehRecorrente: true);
        if (!contratoResult.EhSucesso)
            return contratoResult.Erro!;

        var contrato = contratoResult.Dado!;
        var dataInicio = (request.DataInicio ?? DateTime.UtcNow).Date;
        var dataFim = request.DataFim!.Value.Date;

        var regraResult = RegraReceita.Criar(
            idUsuario, request.Nome, request.Valor, request.Dia!.Value, request.DiaUtil ?? false,
            request.IdCategoria!.Value, request.IdConta!.Value, dataInicio, dataFim);
        if (!regraResult.EhSucesso)
            return regraResult.Erro!;

        var regra = regraResult.Dado!;
        contrato.VincularRegra(regra.Id);

        var meses = LancamentoHelper.GerarMeses(regra.DataInicio, regra.DataFim);
        var receitas = new List<Receita>();
        foreach (var m in meses)
        {
            var criada = Receita.Criar(
                idUsuario, regra.Descricao, regra.Valor,
                LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, m.Mes, m.Ano),
                regra.IdConta, regra.IdCategoria, request.IdSubcategoria, regra.Id,
                idCliente: request.IdCliente, idContrato: contrato.Id);
            if (!criada.EhSucesso)
                return criada.Erro!;

            receitas.Add(criada.Dado!);
        }

        if (receitas.Count == 0)
            return Erro.Negocio("NENHUMA_RECEITA_GERADA", "Nenhuma receita foi gerada para o período informado.");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _repository.InserirAsync(contrato);
        await _regraRepository.InserirAsync(regra);
        await _receitaRepository.InserirEmMassaAsync(receitas);
        scope.Complete();

        var proj = await _repository.ObterProjecaoPorIdAsync(contrato.Id);
        return await MapearComResumoAsync(proj!);
    }

    public async Task<Result<ContratoResponse>> AtualizarAsync(Guid id, Guid idUsuario, ContratoRequest request)
    {
        var contrato = await _repository.ObterPorIdAsync(id);
        if (contrato is null)
            return Erro.NaoEncontrado("Contrato");
        if (contrato.IdUsuario != idUsuario)
            return Erro.Permissao("CONTRATO_ACESSO_NEGADO", "Contrato de outro usuário.");

        var validacao = await ValidarClienteAsync(contrato.IdUsuario, request.IdCliente);
        if (!validacao.EhSucesso)
            return validacao.Erro!;

        var result = contrato.Atualizar(request.Nome, request.IdCliente, request.Valor);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(contrato);
        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return await MapearComResumoAsync(projecao!);
    }

    public async Task<Result<Unit>> EncerrarAsync(Guid id, Guid idUsuario)
    {
        var contrato = await _repository.ObterPorIdAsync(id);
        if (contrato is null)
            return Erro.NaoEncontrado("Contrato");
        if (contrato.IdUsuario != idUsuario)
            return Erro.Permissao("CONTRATO_ACESSO_NEGADO", "Contrato de outro usuário.");
        if (!contrato.Ativo)
            return Erro.Negocio("CONTRATO_JA_ENCERRADO", "Este contrato já está encerrado.");

        var totalRecebido = await _repository.SomarReceitasPorStatusAsync(id, 2);
        if (contrato.Valor - totalRecebido > 0)
            return Erro.Negocio("CONTRATO_COM_PENDENCIAS", "Só é possível encerrar contrato sem valores a receber.");

        contrato.Desativar();
        await _repository.AtualizarAsync(contrato);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ReativarAsync(Guid id, Guid idUsuario)
    {
        var contrato = await _repository.ObterPorIdAsync(id);
        if (contrato is null)
            return Erro.NaoEncontrado("Contrato");
        if (contrato.IdUsuario != idUsuario)
            return Erro.Permissao("CONTRATO_ACESSO_NEGADO", "Contrato de outro usuário.");
        if (contrato.Ativo)
            return Erro.Negocio("CONTRATO_JA_ATIVO", "Este contrato já está ativo.");

        contrato.Reativar();
        await _repository.AtualizarAsync(contrato);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var contrato = await _repository.ObterPorIdAsync(id);
        if (contrato is null)
            return Erro.NaoEncontrado("Contrato");
        if (contrato.IdUsuario != idUsuario)
            return Erro.Permissao("CONTRATO_ACESSO_NEGADO", "Contrato de outro usuário.");

        var totalReceitas = await _repository.SomarReceitasPorStatusAsync(id, 1) + await _repository.SomarReceitasPorStatusAsync(id, 2);
        if (totalReceitas > 0)
            return Erro.Negocio("CONTRATO_COM_VINCULOS", "Não é possível excluir contrato com receitas vinculadas.");

        contrato.Desativar();
        await _repository.AtualizarAsync(contrato);
        return Resultado.Sucesso();
    }

    private async Task<Result<Unit>> ValidarClienteAsync(Guid idUsuario, Guid idCliente)
    {
        var cliente = await _pessoaRepository.ObterPorIdAsync(idCliente);
        if (cliente is null || cliente.IdUsuario != idUsuario)
            return Erro.Validacao("CLIENTE_INVALIDO", "Cliente não encontrado.");
        if (cliente.Tipo != TipoPessoa.Cliente)
            return Erro.Validacao("CLIENTE_TIPO_INVALIDO", "Pessoa selecionada não é um cliente.");
        if (!cliente.Ativo)
            return Erro.Validacao("CLIENTE_INATIVO", "Cliente está inativo.");

        return Resultado.Sucesso();
    }

    private async Task<Result<Unit>> ValidarRecorrenciaAsync(Guid idUsuario, ContratoRequest request)
    {
        if (!request.IdCategoria.HasValue || request.IdCategoria.Value == Guid.Empty)
            return Erro.Validacao("CATEGORIA_OBRIGATORIA", "Categoria é obrigatória para contrato recorrente.");
        if (!request.IdConta.HasValue || request.IdConta.Value == Guid.Empty)
            return Erro.Validacao("CONTA_OBRIGATORIA", "Conta é obrigatória para contrato recorrente.");
        if (!request.Dia.HasValue || request.Dia < 1 || request.Dia > 31)
            return Erro.Validacao("DIA_INVALIDO", "Dia deve estar entre 1 e 31.");
        if (request.DiaUtil == true && request.Dia > 5)
            return Erro.Validacao("DIA_UTIL_INVALIDO", "Dia útil deve estar entre 1 e 5.");
        if (!request.DataFim.HasValue)
            return Erro.Validacao("DATA_FIM_OBRIGATORIA", "Data fim é obrigatória para contrato recorrente.");

        if (_categoriaRepository is not null)
        {
            var cat = await _categoriaRepository.ObterPorIdAsync(request.IdCategoria.Value);
            if (cat is null || cat.IdUsuario != idUsuario || !cat.Ativo)
                return Erro.Validacao("CATEGORIA_INVALIDA", "Categoria não encontrada.");
            if (request.IdSubcategoria.HasValue && request.IdSubcategoria.Value != Guid.Empty)
            {
                var sub = await _categoriaRepository.ObterPorIdAsync(request.IdSubcategoria.Value);
                if (sub is null || sub.IdUsuario != idUsuario || !sub.Ativo)
                    return Erro.Validacao("SUBCATEGORIA_INVALIDA", "Subcategoria não encontrada.");
                if (sub.CategoriaPaiId != request.IdCategoria.Value)
                    return Erro.Validacao("SUBCATEGORIA_INVALIDA", "Subcategoria não pertence à categoria informada.");
            }
        }

        if (_contaRepository is not null)
        {
            var conta = await _contaRepository.ObterPorIdAsync(request.IdConta.Value);
            if (conta is null || conta.IdUsuario != idUsuario || !conta.Ativo)
                return Erro.Validacao("CONTA_INVALIDA", "Conta não encontrada.");
        }

        var dataInicio = (request.DataInicio ?? DateTime.UtcNow).Date;
        if (request.DataFim!.Value.Date < dataInicio)
            return Erro.Validacao("PERIODO_INVALIDO", "Data fim deve ser posterior à data início.");

        return Resultado.Sucesso();
    }

    private async Task<ContratoResponse> MapearComResumoAsync(ContratoProjecao c, decimal? totalRecebido = null)
    {
        totalRecebido ??= await _repository.SomarReceitasPorStatusAsync(c.Id, (int)StatusMensal.Realizado);
        return new ContratoResponse
        {
            Id = c.Id,
            Nome = c.Nome,
            IdCliente = c.IdCliente,
            Cliente = c.Cliente,
            Valor = c.Valor,
            Ativo = c.Ativo,
            EhRecorrente = c.EhRecorrente,
            IdRegra = c.IdRegra,
            DataCadastro = c.DataCadastro,
            TotalRecebido = totalRecebido.Value,
            FaltaReceber = c.Valor - totalRecebido.Value
        };
    }
}
