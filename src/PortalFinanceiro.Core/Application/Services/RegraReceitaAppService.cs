using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;
using PortalFinanceiro.Core.Domain.Services;

namespace PortalFinanceiro.Core.Application.Services;

public class RegraReceitaAppService : IRegraReceitaAppService
{
    private readonly IRegraReceitaRepository _regraRepository;
    private readonly IReceitaRepository _receitaRepository;

    public RegraReceitaAppService(IRegraReceitaRepository regraRepository, IReceitaRepository receitaRepository)
    {
        _regraRepository = regraRepository;
        _receitaRepository = receitaRepository;
    }

    public async Task<Result<IEnumerable<RegraReceitaResponse>>> ListarAsync(Guid idUsuario)
    {
        var regras = await _regraRepository.ListarPorUsuarioAsync(idUsuario);
        return regras.Select(Mapear).ToList();
    }

    public async Task<Result<RegraReceitaResponse>> ObterPorIdAsync(Guid id)
    {
        var regra = await _regraRepository.ObterPorIdAsync(id);
        if (regra is null)
            return Erro.NaoEncontrado("Regra de receita");

        return Mapear(regra);
    }

    public async Task<Result<RegraReceitaResponse>> AtualizarAsync(Guid id, RegraReceitaRequest request)
    {
        var regra = await _regraRepository.ObterPorIdAsync(id);
        if (regra is null)
            return Erro.NaoEncontrado("Regra de receita");

        var result = regra.Atualizar(request.Descricao, request.Valor, request.Dia, request.DiaUtil, request.IdCategoria, request.IdConta, request.DataInicio, request.DataFim);
        if (!result.EhSucesso)
            return result.Erro!;

        await _regraRepository.AtualizarAsync(regra);

        var agora = DateTime.UtcNow;
        var parcelas = await _receitaRepository.ListarPorRegraAsync(regra.Id);

        foreach (var parcela in parcelas.Where(p => p.Status == StatusMensal.Pendente && p.Data >= agora))
        {
            var dataVencimento = LancamentoHelper.CalcularDataVencimento(regra.Dia, regra.DiaUtil, parcela.Data.Month, parcela.Data.Year);
            var atualizar = parcela.Atualizar(regra.Descricao, regra.Valor, dataVencimento, regra.IdConta, regra.IdCategoria, parcela.IdSubcategoria);
            if (atualizar.EhSucesso)
                await _receitaRepository.AtualizarAsync(parcela);
        }

        return Mapear(regra);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var regra = await _regraRepository.ObterPorIdAsync(id);
        if (regra is null)
            return Erro.NaoEncontrado("Regra de receita");

        regra.Desativar();
        await _regraRepository.AtualizarAsync(regra);

        var agora = DateTime.UtcNow;
        var parcelas = await _receitaRepository.ListarPorRegraAsync(regra.Id);
        foreach (var parcela in parcelas.Where(p => p.Status == StatusMensal.Pendente && p.Data >= agora))
        {
            parcela.Desativar();
            await _receitaRepository.AtualizarAsync(parcela);
        }

        return Resultado.Sucesso();
    }

    private static RegraReceitaResponse Mapear(RegraReceita r) => new()
    {
        Id = r.Id,
        Descricao = r.Descricao,
        Valor = r.Valor,
        Dia = r.Dia,
        DiaUtil = r.DiaUtil,
        IdCategoria = r.IdCategoria,
        Categoria = r.Categoria,
        IdConta = r.IdConta,
        Conta = r.Conta,
        DataInicio = r.DataInicio,
        DataFim = r.DataFim,
        Ativo = r.Ativo
    };
}
