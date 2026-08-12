using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class CategoriaReceitaAppService : ICategoriaReceitaAppService
{
    private readonly ICategoriaReceitaRepository _repository;
    private readonly IReceitaRepository _receitaRepository;
    private readonly ICategoriaHistoricoRepository _historicoRepository;

    public CategoriaReceitaAppService(
        ICategoriaReceitaRepository repository,
        IReceitaRepository receitaRepository,
        ICategoriaHistoricoRepository historicoRepository)
    {
        _repository = repository;
        _receitaRepository = receitaRepository;
        _historicoRepository = historicoRepository;
    }

    public async Task<Result<IEnumerable<CategoriaResponse>>> ListarAsync(Guid idUsuario, bool isAdmin)
    {
        var categorias = await _repository.ListarAsync();
        return categorias.Select(c => Mapear(c, idUsuario, isAdmin)).ToList();
    }

    public async Task<Result<CategoriaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario, bool isAdmin)
    {
        var categoria = await _repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        return Mapear(categoria, idUsuario, isAdmin);
    }

    public async Task<Result<CategoriaResponse>> AdicionarAsync(Guid idUsuario, CategoriaRequest request)
    {
        if (request is null)
            return Erro.Validacao("REQUISICAO_INVALIDA", "Corpo da requisição é obrigatório.");

        var result = CategoriaReceita.Criar(idUsuario, request.Nome, request.CategoriaPaiId);
        if (!result.EhSucesso)
            return result.Erro!;

        var categoria = result.Dado!;
        await _repository.InserirAsync(categoria);
        await RegistrarHistoricoAsync(categoria, EAcaoCategoriaHistorico.Criado, nomeNovo: categoria.Nome, categoriaPaiIdNova: categoria.CategoriaPaiId);

        return Mapear(categoria, idUsuario, false);
    }

    public async Task<Result<CategoriaResponse>> AtualizarAsync(Guid id, Guid idUsuario, bool isAdmin, CategoriaRequest request)
    {
        var categoria = await _repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        if (!PodeGerenciar(categoria, idUsuario, isAdmin))
            return Erro.Permissao("SEM_PERMISSAO", "Você não tem permissão para editar esta categoria.");

        if (request is null)
            return Erro.Validacao("REQUISICAO_INVALIDA", "Corpo da requisição é obrigatório.");

        var nomeAntigo = categoria.Nome;
        var paiAntigo = categoria.CategoriaPaiId;

        var result = categoria.Atualizar(request.Nome, request.CategoriaPaiId);
        if (!result.EhSucesso)
            return result.Erro!;

        await _repository.AtualizarAsync(categoria);
        await RegistrarHistoricoAsync(categoria, EAcaoCategoriaHistorico.Editado, nomeAntigo, categoria.Nome, paiAntigo, categoria.CategoriaPaiId);

        return Mapear(categoria, idUsuario, isAdmin);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario, bool isAdmin)
    {
        var categoria = await _repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        if (!PodeGerenciar(categoria, idUsuario, isAdmin))
            return Erro.Permissao("SEM_PERMISSAO", "Você não tem permissão para excluir esta categoria.");

        var vinculadas = await _receitaRepository.ContarPorCategoriaAsync(id);
        if (vinculadas > 0)
            return Erro.Negocio("CATEGORIA_COM_VINCULOS", $"Não é possível excluir. Existem {vinculadas} receita(s) vinculada(s) a esta categoria.");

        var subVinculadas = await _receitaRepository.ContarPorSubcategoriaAsync(id);
        if (subVinculadas > 0)
            return Erro.Negocio("CATEGORIA_COM_SUB_VINCULOS", $"Não é possível excluir. Existem {subVinculadas} receita(s) vinculada(s) a subcategorias desta categoria.");

        var subcategorias = await _repository.ListarPorPaiAsync(id);
        foreach (var sub in subcategorias)
        {
            var subVinculos = await _receitaRepository.ContarPorCategoriaAsync(sub.Id);
            if (subVinculos > 0)
                return Erro.Negocio("SUBCATEGORIA_COM_VINCULOS", $"Não é possível excluir. A subcategoria \"{sub.Nome}\" possui {subVinculos} receita(s) vinculada(s).");
        }

        categoria.Desativar();
        await _repository.ExcluirAsync(categoria.Id);
        await RegistrarHistoricoAsync(categoria, EAcaoCategoriaHistorico.Excluido, nomeAntigo: categoria.Nome, categoriaPaiIdAntiga: categoria.CategoriaPaiId);

        foreach (var sub in subcategorias)
        {
            var subVinculos = await _receitaRepository.ContarPorCategoriaAsync(sub.Id);
            sub.Desativar();
            await _repository.ExcluirAsync(sub.Id);
            await RegistrarHistoricoAsync(sub, EAcaoCategoriaHistorico.Excluido, nomeAntigo: sub.Nome, categoriaPaiIdAntiga: sub.CategoriaPaiId);
        }

        return Resultado.Sucesso();
    }

    private bool PodeGerenciar(CategoriaReceita categoria, Guid idUsuario, bool isAdmin)
        => categoria.IdUsuario == idUsuario || isAdmin;

    private async Task RegistrarHistoricoAsync(
        CategoriaReceita categoria,
        EAcaoCategoriaHistorico acao,
        string? nomeAntigo = null,
        string? nomeNovo = null,
        Guid? categoriaPaiIdAntiga = null,
        Guid? categoriaPaiIdNova = null)
    {
        var historico = CategoriaHistorico.Criar(categoria.Id, ETipoCategoria.Receita, categoria.IdUsuario, acao, nomeAntigo, nomeNovo, categoriaPaiIdAntiga, categoriaPaiIdNova);
        await _historicoRepository.InserirAsync(historico);
    }

    private static CategoriaResponse Mapear(CategoriaReceita c, Guid idUsuario, bool isAdmin) => new()
    {
        Id = c.Id,
        IdUsuario = c.IdUsuario,
        Nome = c.Nome,
        CategoriaPaiId = c.CategoriaPaiId,
        Ativo = c.Ativo,
        PodeEditar = c.IdUsuario == idUsuario || isAdmin,
        DataCadastro = c.DataCadastro
    };
}