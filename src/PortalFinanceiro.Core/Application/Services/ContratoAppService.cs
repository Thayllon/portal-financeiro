using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class ContratoAppService : IContratoAppService
{
    private readonly IContratoRepository _repository;
    private readonly IPessoaRepository _pessoaRepository;

    public ContratoAppService(IContratoRepository repository, IPessoaRepository pessoaRepository)
    {
        _repository = repository;
        _pessoaRepository = pessoaRepository;
    }

    public async Task<Result<IEnumerable<ContratoResponse>>> ListarAsync(Guid idUsuario, bool? ativo = null)
    {
        var contratos = await _repository.ListarAsync(idUsuario, ativo);
        var responses = new List<ContratoResponse>();
        foreach (var c in contratos)
            responses.Add(await MapearComResumoAsync(c));
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

        var result = Contrato.Criar(idUsuario, request.Nome, request.IdCliente, request.Valor);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.InserirAsync(result.Dado!);
        var projecao = await _repository.ObterProjecaoPorIdAsync(result.Dado!.Id);
        return await MapearComResumoAsync(projecao!);
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

    private async Task<ContratoResponse> MapearComResumoAsync(ContratoProjecao c)
    {
        var totalRecebido = await _repository.SomarReceitasPorStatusAsync(c.Id, 2);
        return new ContratoResponse
        {
            Id = c.Id,
            Nome = c.Nome,
            IdCliente = c.IdCliente,
            Cliente = c.Cliente,
            Valor = c.Valor,
            Ativo = c.Ativo,
            DataCadastro = c.DataCadastro,
            TotalRecebido = totalRecebido,
            FaltaReceber = c.Valor - totalRecebido
        };
    }
}
