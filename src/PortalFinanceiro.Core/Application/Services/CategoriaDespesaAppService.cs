using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class CategoriaDespesaAppService : ICategoriaDespesaAppService
{
    private readonly ICategoriaDespesaRepository _repository;
    private readonly IDespesaRepository _despesaRepository;
    private readonly ICategoriaHistoricoRepository _historicoRepository;

    public CategoriaDespesaAppService(
        ICategoriaDespesaRepository repository,
        IDespesaRepository despesaRepository,
        ICategoriaHistoricoRepository historicoRepository)
    {
        _repository = repository;
        _despesaRepository = despesaRepository;
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

        var result = CategoriaDespesa.Criar(idUsuario, request.Nome, request.CategoriaPaiId);
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
            return Erro.Permissao("SEM_PERMISSAO", "Apenas o proprietário da categoria ou um administrador pode excluí-la.");

        var vinculadas = await _despesaRepository.ContarPorCategoriaAsync(id);
        if (vinculadas > 0)
            return Erro.Negocio("CATEGORIA_COM_VINCULOS", $"Não é possível excluir a categoria \"{categoria.Nome}\": há {vinculadas} despesa(s) vinculada(s) diretamente a ela. Remova ou reatribua essas despesas antes de excluir.");

        var subVinculadas = await _despesaRepository.ContarPorSubcategoriaAsync(id);
        if (subVinculadas > 0)
            return Erro.Negocio("CATEGORIA_COM_SUB_VINCULOS", $"Não é possível excluir a categoria \"{categoria.Nome}\": há {subVinculadas} despesa(s) vinculada(s) às subcategorias dela. Remova ou reatribua antes de excluir.");

        var subcategorias = await _repository.ListarPorPaiAsync(id);
        foreach (var sub in subcategorias)
        {
            var subVinculos = await _despesaRepository.ContarPorCategoriaAsync(sub.Id);
            if (subVinculos > 0)
                return Erro.Negocio("SUBCATEGORIA_COM_VINCULOS", $"Não é possível excluir: a subcategoria \"{sub.Nome}\" possui {subVinculos} despesa(s) vinculada(s). Remova ou reatribua antes de excluir.");
        }

        categoria.Desativar();
        await _repository.ExcluirAsync(categoria.Id);
        await RegistrarHistoricoAsync(categoria, EAcaoCategoriaHistorico.Excluido, nomeAntigo: categoria.Nome, categoriaPaiIdAntiga: categoria.CategoriaPaiId);

        foreach (var sub in subcategorias)
        {
            sub.Desativar();
            await _repository.ExcluirAsync(sub.Id);
            await RegistrarHistoricoAsync(sub, EAcaoCategoriaHistorico.Excluido, nomeAntigo: sub.Nome, categoriaPaiIdAntiga: sub.CategoriaPaiId);
        }

        return Resultado.Sucesso();
    }

    private bool PodeGerenciar(CategoriaDespesa categoria, Guid idUsuario, bool isAdmin)
        => categoria.IdUsuario == idUsuario || isAdmin;

    private async Task RegistrarHistoricoAsync(
        CategoriaDespesa categoria,
        EAcaoCategoriaHistorico acao,
        string? nomeAntigo = null,
        string? nomeNovo = null,
        Guid? categoriaPaiIdAntiga = null,
        Guid? categoriaPaiIdNova = null)
    {
        var historico = CategoriaHistorico.Criar(categoria.Id, ETipoCategoria.Despesa, categoria.IdUsuario, acao, nomeAntigo, nomeNovo, categoriaPaiIdAntiga, categoriaPaiIdNova);
        await _historicoRepository.InserirAsync(historico);
    }

    private static CategoriaResponse Mapear(CategoriaDespesa c, Guid idUsuario, bool isAdmin) => new()
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