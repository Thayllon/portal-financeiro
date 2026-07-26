using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class CategoriaDespesaAppService : ICategoriaDespesaAppService
{
    private readonly ICategoriaDespesaRepository _repository;

    public CategoriaDespesaAppService(ICategoriaDespesaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<CategoriaResponse>>> ListarAsync(Guid idUsuario)
    {
        var categorias = await _repository.ListarPorUsuarioAsync(idUsuario);
        return categorias.Select(Mapear).ToList();
    }

    public async Task<Result<CategoriaResponse>> ObterPorIdAsync(Guid id)
    {
        var categoria = await _repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        return Mapear(categoria);
    }

    public async Task<Result<CategoriaResponse>> AdicionarAsync(Guid idUsuario, CategoriaRequest request)
    {
        var result = CategoriaDespesa.Criar(idUsuario, request.Nome);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.InserirAsync(result.Dado!);
        return Mapear(result.Dado!);
    }

    public async Task<Result<CategoriaResponse>> AtualizarAsync(Guid id, CategoriaRequest request)
    {
        var categoria = await _repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        var result = categoria.Atualizar(request.Nome);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(categoria);
        return Mapear(categoria);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id)
    {
        var categoria = await _repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        categoria.Desativar();
        await _repository.AtualizarAsync(categoria);
        return Resultado.Sucesso();
    }

    private static CategoriaResponse Mapear(CategoriaDespesa c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Ativo = c.Ativo,
        DataCadastro = c.DataCadastro
    };
}
