using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Core.Domain.Results;
using PortalFinanceiro.Core.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PortalFinanceiro.Core.Application.Services;

public class RegraDespesaAppService : IRegraDespesaAppService
{
    private readonly IRegraDespesaRepository _regraRepository;
    private readonly IDespesaRepository _despesaRepository;
    private readonly ILogger<RegraDespesaAppService> _logger;

    public RegraDespesaAppService(IRegraDespesaRepository regraRepository, IDespesaRepository despesaRepository, ILogger<RegraDespesaAppService>? logger = null)
    {
        _regraRepository = regraRepository;
        _despesaRepository = despesaRepository;
        _logger = logger ?? NullLogger<RegraDespesaAppService>.Instance;
    }

    public async Task<Result<IEnumerable<RegraDespesaResponse>>> ListarAsync(Guid idUsuario)
    {
        var regras = await _regraRepository.ListarPorUsuarioAsync(idUsuario);
        return regras.Select(Mapear).ToList();
    }

    public async Task<Result<RegraDespesaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var regra = await _regraRepository.ObterProjecaoPorIdAsync(id);
        if (regra is null)
            return Erro.NaoEncontrado("Regra de despesa");
        if (regra.IdUsuario != idUsuario)
            return Erro.Permissao("REGRA_DESPESA_ACESSO_NEGADO", "Regra de despesa de outro usuário.");

        return Mapear(regra);
    }

    public async Task<Result<RegraDespesaResponse>> AtualizarAsync(Guid id, Guid idUsuario, RegraDespesaRequest request)
    {
        var regra = await _regraRepository.ObterPorIdAsync(id);
        if (regra is null)
            return Erro.NaoEncontrado("Regra de despesa");
        if (regra.IdUsuario != idUsuario)
            return Erro.Permissao("REGRA_DESPESA_ACESSO_NEGADO", "Regra de despesa de outro usuário.");

        var result = regra.Atualizar(request.Descricao, request.Valor, request.Dia, request.DiaUtil, request.IdCategoria, request.IdConta, request.DataInicio, request.DataFim);
        if (!result.EhSucesso)
            return result.Erro!;

        regra.DefinirEditor(idUsuario);
        await _regraRepository.AtualizarAsync(regra);
        _logger.Auditar(idUsuario, "Atualizar", "RegraDespesa", regra.Id);

        var agora = DateTime.UtcNow;
        var parcelas = await _despesaRepository.ListarPorRegraAsync(regra.Id);

        foreach (var parcela in parcelas.Where(p => p.Status == StatusMensal.Pendente && p.Data >= agora))
        {
            var dataVencimento = LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, parcela.Data.Month, parcela.Data.Year);
            var atualizar = parcela.Atualizar(regra.Descricao, regra.Valor, dataVencimento, regra.IdConta, regra.IdCategoria, parcela.IdSubcategoria);
            if (!atualizar.EhSucesso)
                return atualizar.Erro!;

            parcela.DefinirEditor(idUsuario);
            await _despesaRepository.AtualizarAsync(parcela);
        }

        var projecao = await _regraRepository.ObterProjecaoPorIdAsync(id);
        return Mapear(projecao!);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var regra = await _regraRepository.ObterPorIdAsync(id);
        if (regra is null)
            return Erro.NaoEncontrado("Regra de despesa");
        if (regra.IdUsuario != idUsuario)
            return Erro.Permissao("REGRA_DESPESA_ACESSO_NEGADO", "Regra de despesa de outro usuário.");

        regra.Desativar();
        regra.DefinirEditor(idUsuario);
        await _regraRepository.AtualizarAsync(regra);
        _logger.Auditar(idUsuario, "Excluir", "RegraDespesa", regra.Id);

        var agora = DateTime.UtcNow;
        var parcelas = await _despesaRepository.ListarPorRegraAsync(regra.Id);
        foreach (var parcela in parcelas.Where(p => p.Status == StatusMensal.Pendente && p.Data >= agora))
        {
            parcela.Desativar();
            parcela.DefinirEditor(idUsuario);
            await _despesaRepository.AtualizarAsync(parcela);
        }

        return Resultado.Sucesso();
    }

    private static RegraDespesaResponse Mapear(RegraDespesaProjecao p) => new()
    {
        Id = p.Id,
        Descricao = p.Descricao,
        Valor = p.Valor,
        Dia = p.Dia,
        DiaUtil = p.DiaUtil,
        IdCategoria = p.IdCategoria,
        Categoria = p.Categoria,
        IdConta = p.IdConta,
        Conta = p.Conta,
        DataInicio = p.DataInicio,
        DataFim = p.DataFim,
        Ativo = p.Ativo,
        CriadoPor = p.CriadoPor,
        AlteradoPor = p.AlteradoPor
    };
}
