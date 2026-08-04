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
    private readonly IDespesaRepository _despesaRepository;

    public CategoriaDespesaAppService(ICategoriaDespesaRepository repository, IDespesaRepository despesaRepository)
    {
        _repository = repository;
        _despesaRepository = despesaRepository;
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
        var result = CategoriaDespesa.Criar(idUsuario, request.Nome, request.CategoriaPaiId);
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

        var result = categoria.Atualizar(request.Nome, request.CategoriaPaiId);
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

        var vinculadas = await _despesaRepository.ContarPorCategoriaAsync(id);
        if (vinculadas > 0)
            return Erro.Negocio("CATEGORIA_COM_VINCULOS", $"Não é possível excluir. Existem {vinculadas} despesa(s) vinculada(s) a esta categoria.");

        var subVinculadas = await _despesaRepository.ContarPorSubcategoriaAsync(id);
        if (subVinculadas > 0)
            return Erro.Negocio("CATEGORIA_COM_SUB_VINCULOS", $"Não é possível excluir. Existem {subVinculadas} despesa(s) vinculada(s) a subcategorias desta categoria.");

        var subcategorias = await _repository.ListarPorPaiAsync(id);
        foreach (var sub in subcategorias)
        {
            var subVinculos = await _despesaRepository.ContarPorCategoriaAsync(sub.Id);
            if (subVinculos > 0)
                return Erro.Negocio("SUBCATEGORIA_COM_VINCULOS", $"Não é possível excluir. A subcategoria \"{sub.Nome}\" possui {subVinculos} despesa(s) vinculada(s).");
        }

        categoria.Desativar();
        await _repository.ExcluirAsync(categoria.Id);

        foreach (var sub in subcategorias)
            await _repository.ExcluirAsync(sub.Id);

        return Resultado.Sucesso();
    }

    private static CategoriaResponse Mapear(CategoriaDespesa c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        CategoriaPaiId = c.CategoriaPaiId,
        Ativo = c.Ativo,
        DataCadastro = c.DataCadastro
    };
}
