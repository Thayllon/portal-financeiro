using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ParceriaAppService : IParceriaAppService
{
    private readonly IParceriaRepository _repository;
    private readonly IPessoaRepository _pessoaRepository;
    private readonly IProcessoRepository? _processoRepository;
    private readonly ILogger<ParceriaAppService> _logger;

    public ParceriaAppService(IParceriaRepository repository, IPessoaRepository pessoaRepository, IProcessoRepository? processoRepository = null, ILogger<ParceriaAppService>? logger = null)
    {
        _repository = repository;
        _pessoaRepository = pessoaRepository;
        _processoRepository = processoRepository;
        _logger = logger ?? NullLogger<ParceriaAppService>.Instance;
    }

    public async Task<Result<IEnumerable<ParceriaResponse>>> ListarAsync(Guid idUsuario, bool? ativo = null)
    {
        var parcerias = await _repository.ListarComTotaisAsync(idUsuario, ativo, (int)StatusMensal.Realizado);
        var responses = new List<ParceriaResponse>();
        foreach (var p in parcerias)
            responses.Add(await MapearComResumoAsync(p, p.TotalRecebido, p.TotalPago));
        return responses;
    }

    public async Task<Result<ParceriaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario)
    {
        var parceria = await _repository.ObterProjecaoPorIdAsync(id);
        if (parceria is null)
            return Erro.NaoEncontrado("Parceria");
        if (parceria.IdUsuario != idUsuario)
            return Erro.Permissao("PARCERIA_ACESSO_NEGADO", "Parceria de outro usuário.");
        return await MapearComResumoAsync(parceria);
    }

    public async Task<Result<ParceriaResponse>> AdicionarAsync(Guid idUsuario, ParceriaRequest request)
    {
        var validacao = await ValidarPessoasAsync(idUsuario, request.IdParceiro, request.IdCliente);
        if (!validacao.EhSucesso)
            return validacao.Erro!;

        var result = Parceria.Criar(idUsuario, request.Nome, request.IdParceiro, request.IdCliente, request.Valor, request.PercentualParceiro);
        if (!result.EhSucesso)
            return result.Erro!;

        result.Dado!.DefinirCriador(idUsuario);
        await _repository.InserirAsync(result.Dado!);
        _logger.Auditar(idUsuario, "Criar", "Parceria", result.Dado!.Id);
        var projecao = await _repository.ObterProjecaoPorIdAsync(result.Dado!.Id);
        return await MapearComResumoAsync(projecao!);
    }

    public async Task<Result<ParceriaResponse>> AtualizarAsync(Guid id, Guid idUsuario, ParceriaRequest request)
    {
        var parceria = await _repository.ObterPorIdAsync(id);
        if (parceria is null)
            return Erro.NaoEncontrado("Parceria");
        if (parceria.IdUsuario != idUsuario)
            return Erro.Permissao("PARCERIA_ACESSO_NEGADO", "Parceria de outro usuário.");

        var validacao = await ValidarPessoasAsync(parceria.IdUsuario, request.IdParceiro, request.IdCliente);
        if (!validacao.EhSucesso)
            return validacao.Erro!;

        var result = parceria.Atualizar(request.Nome, request.IdParceiro, request.IdCliente, request.Valor, request.PercentualParceiro);
        if (!result.EhSucesso)
            return result.Erro!;

        parceria.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(parceria);
        _logger.Auditar(idUsuario, "Atualizar", "Parceria", parceria.Id);
        var projecao = await _repository.ObterProjecaoPorIdAsync(id);
        return await MapearComResumoAsync(projecao!);
    }

    public async Task<Result<Unit>> EncerrarAsync(Guid id, Guid idUsuario)
    {
        var parceria = await _repository.ObterPorIdAsync(id);
        if (parceria is null)
            return Erro.NaoEncontrado("Parceria");
        if (parceria.IdUsuario != idUsuario)
            return Erro.Permissao("PARCERIA_ACESSO_NEGADO", "Parceria de outro usuário.");
        if (!parceria.Ativo)
            return Erro.Negocio("PARCERIA_JA_ENCERRADA", "Esta parceria já está encerrada.");

        var totalRecebido = await _repository.SomarReceitasPorStatusAsync(id, (int)StatusMensal.Realizado);
        var totalPago = await _repository.SomarDespesasPorStatusAsync(id, (int)StatusMensal.Realizado);
        var valorParceiro = Math.Round(parceria.Valor * parceria.PercentualParceiro / 100, 2);
        if (parceria.Valor - totalRecebido > 0 || valorParceiro - totalPago > 0)
            return Erro.Negocio("PARCERIA_COM_PENDENCIAS", "Só é possível encerrar parceria sem valores a receber e a pagar.");

        parceria.Desativar();
        parceria.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(parceria);
        _logger.Auditar(idUsuario, "Encerrar", "Parceria", parceria.Id);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ReativarAsync(Guid id, Guid idUsuario)
    {
        var parceria = await _repository.ObterPorIdAsync(id);
        if (parceria is null)
            return Erro.NaoEncontrado("Parceria");
        if (parceria.IdUsuario != idUsuario)
            return Erro.Permissao("PARCERIA_ACESSO_NEGADO", "Parceria de outro usuário.");
        if (parceria.Ativo)
            return Erro.Negocio("PARCERIA_JA_ATIVA", "Esta parceria já está ativa.");

        parceria.Reativar();
        parceria.DefinirEditor(idUsuario);
        await _repository.AtualizarAsync(parceria);
        _logger.Auditar(idUsuario, "Reativar", "Parceria", parceria.Id);
        return Resultado.Sucesso();
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario)
    {
        var parceria = await _repository.ObterPorIdAsync(id);
        if (parceria is null)
            return Erro.NaoEncontrado("Parceria");
        if (parceria.IdUsuario != idUsuario)
            return Erro.Permissao("PARCERIA_ACESSO_NEGADO", "Parceria de outro usuário.");

        var totalReceitas = await _repository.SomarReceitasPorStatusAsync(id, (int)StatusMensal.Pendente) + await _repository.SomarReceitasPorStatusAsync(id, (int)StatusMensal.Realizado);
        var totalDespesas = await _repository.SomarDespesasPorStatusAsync(id, (int)StatusMensal.Pendente) + await _repository.SomarDespesasPorStatusAsync(id, (int)StatusMensal.Realizado);
        if (totalReceitas > 0 || totalDespesas > 0)
            return Erro.Negocio("PARCERIA_COM_VINCULOS", "Não é possível excluir parceria com receitas ou despesas vinculadas.");

        if (_processoRepository is not null)
        {
            if (await _processoRepository.ContarAtivosPorParceriaAsync(id) > 0)
                return Erro.Negocio("PARCERIA_COM_PROCESSOS", "Não é possível excluir parceria com processos ativos vinculados.");
            if (await _processoRepository.ContarPorParceriaAsync(id) > 0)
                return Erro.Negocio("PARCERIA_COM_PROCESSOS", "Não é possível excluir parceria com processos vinculados. Exclua os processos primeiro.");
        }

        await _repository.ExcluirAsync(id);
        _logger.Auditar(idUsuario, "Excluir", "Parceria", id);
        return Resultado.Sucesso();
    }

    private async Task<Result<Unit>> ValidarPessoasAsync(Guid idUsuario, Guid idParceiro, Guid idCliente)
    {
        var parceiro = await _pessoaRepository.ObterPorIdAsync(idParceiro);
        if (parceiro is null || parceiro.IdUsuario != idUsuario)
            return Erro.Validacao("PARCEIRO_INVALIDO", "Parceiro não encontrado.");
        if (parceiro.Tipo != TipoPessoa.Parceiro)
            return Erro.Validacao("PARCEIRO_TIPO_INVALIDO", "Pessoa selecionada não é um parceiro.");
        if (!parceiro.Ativo)
            return Erro.Validacao("PARCEIRO_INATIVO", "Parceiro está inativo.");

        var cliente = await _pessoaRepository.ObterPorIdAsync(idCliente);
        if (cliente is null || cliente.IdUsuario != idUsuario)
            return Erro.Validacao("CLIENTE_INVALIDO", "Cliente não encontrado.");
        if (cliente.Tipo != TipoPessoa.Cliente)
            return Erro.Validacao("CLIENTE_TIPO_INVALIDO", "Pessoa selecionada não é um cliente.");
        if (!cliente.Ativo)
            return Erro.Validacao("CLIENTE_INATIVO", "Cliente está inativo.");

        return Resultado.Sucesso();
    }

    private async Task<ParceriaResponse> MapearComResumoAsync(ParceriaProjecao p, decimal? totalRecebido = null, decimal? totalPago = null)
    {
        totalRecebido ??= await _repository.SomarReceitasPorStatusAsync(p.Id, (int)StatusMensal.Realizado);
        totalPago ??= await _repository.SomarDespesasPorStatusAsync(p.Id, (int)StatusMensal.Realizado);
        var valorParceiro = Math.Round(p.Valor * p.PercentualParceiro / 100, 2);
        return new ParceriaResponse
        {
            Id = p.Id,
            Nome = p.Nome,
            IdParceiro = p.IdParceiro,
            Parceiro = p.Parceiro,
            IdCliente = p.IdCliente,
            Cliente = p.Cliente,
            Valor = p.Valor,
            PercentualParceiro = p.PercentualParceiro,
            ValorParceiro = valorParceiro,
            MinhaParte = p.Valor - valorParceiro,
            Ativo = p.Ativo,
            DataCadastro = p.DataCadastro,
            CriadoPor = p.CriadoPor,
            AlteradoPor = p.AlteradoPor,
            TotalRecebido = totalRecebido.Value,
            TotalPago = totalPago.Value,
            FaltaReceber = p.Valor - totalRecebido.Value,
            FaltaPagar = valorParceiro - totalPago.Value
        };
    }
}
