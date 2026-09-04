using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public abstract class CategoriaBaseAppService<T> where T : class
{
    protected readonly ICategoriaRepository<T> Repository;
    protected readonly ICategoriaHistoricoRepository HistoricoRepository;
    protected readonly ETipoCategoria TipoCategoria;

    protected CategoriaBaseAppService(
        ICategoriaRepository<T> repository,
        ICategoriaHistoricoRepository historicoRepository,
        ETipoCategoria tipoCategoria)
    {
        Repository = repository;
        HistoricoRepository = historicoRepository;
        TipoCategoria = tipoCategoria;
    }

    public async Task<Result<IEnumerable<CategoriaResponse>>> ListarAsync(Guid idUsuario, bool isAdmin)
    {
        var categorias = await Repository.ListarAsync();
        return categorias.Select(c => Mapear(c, idUsuario, isAdmin)).ToList();
    }

    public async Task<Result<CategoriaResponse>> ObterPorIdAsync(Guid id, Guid idUsuario, bool isAdmin)
    {
        var categoria = await Repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        return Mapear(categoria, idUsuario, isAdmin);
    }

    public async Task<Result<CategoriaResponse>> AdicionarAsync(Guid idUsuario, CategoriaRequest request)
    {
        if (request is null)
            return Erro.Validacao("REQUISICAO_INVALIDA", "Corpo da requisição é obrigatório.");

        var result = CriarEntidade(idUsuario, request);
        if (!result.EhSucesso)
            return result.Erro!;

        var categoria = result.Dado!;
        await Repository.InserirAsync(categoria);
        await RegistrarHistoricoAsync(categoria, EAcaoCategoriaHistorico.Criado, nomeNovo: ObterNome(categoria), categoriaPaiIdNova: ObterCategoriaPaiId(categoria));

        return Mapear(categoria, idUsuario, false);
    }

    public async Task<Result<CategoriaResponse>> AtualizarAsync(Guid id, Guid idUsuario, bool isAdmin, CategoriaRequest request)
    {
        var categoria = await Repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        if (!PodeGerenciar(categoria, idUsuario, isAdmin))
            return Erro.Permissao("SEM_PERMISSAO", "Você não tem permissão para editar esta categoria.");

        if (request is null)
            return Erro.Validacao("REQUISICAO_INVALIDA", "Corpo da requisição é obrigatório.");

        var nomeAntigo = ObterNome(categoria);
        var paiAntigo = ObterCategoriaPaiId(categoria);

        var result = AtualizarEntidade(categoria, request);
        if (!result.EhSucesso)
            return result.Erro!;

        await Repository.AtualizarAsync(categoria);
        await RegistrarHistoricoAsync(categoria, EAcaoCategoriaHistorico.Editado, nomeAntigo, ObterNome(categoria), paiAntigo, ObterCategoriaPaiId(categoria));

        return Mapear(categoria, idUsuario, isAdmin);
    }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario, bool isAdmin, Func<Guid, Task<int>> contarVinculos)
    {
        var categoria = await Repository.ObterPorIdAsync(id);
        if (categoria is null)
            return Erro.NaoEncontrado("Categoria");

        if (!PodeGerenciar(categoria, idUsuario, isAdmin))
            return Erro.Permissao("SEM_PERMISSAO", "Apenas o proprietário da categoria ou um administrador pode excluí-la.");

        var vinculadas = await contarVinculos(id);
        if (vinculadas > 0)
            return Erro.Negocio("CATEGORIA_COM_VINCULOS", $"Não é possível excluir a categoria \"{ObterNome(categoria)}\": há {vinculadas} registro(s) vinculado(s) diretamente a ele. Remova ou reatribua antes de excluir.");

        var subcategorias = await Repository.ListarPorPaiAsync(id);
        foreach (var sub in subcategorias)
        {
            var subVinculos = await contarVinculos(ObterId(sub));
            if (subVinculos > 0)
                return Erro.Negocio("SUBCATEGORIA_COM_VINCULOS", $"Não é possível excluir: a subcategoria \"{ObterNome(sub)}\" possui {subVinculos} registro(s) vinculado(s). Remova ou reatribua antes de excluir.");
        }

        DesativarEntidade(categoria);
        await Repository.ExcluirAsync(ObterId(categoria));
        await RegistrarHistoricoAsync(categoria, EAcaoCategoriaHistorico.Excluido, nomeAntigo: ObterNome(categoria), categoriaPaiIdAntiga: ObterCategoriaPaiId(categoria));

        foreach (var sub in subcategorias)
        {
            DesativarEntidade(sub);
            await Repository.ExcluirAsync(ObterId(sub));
            await RegistrarHistoricoAsync(sub, EAcaoCategoriaHistorico.Excluido, nomeAntigo: ObterNome(sub), categoriaPaiIdAntiga: ObterCategoriaPaiId(sub));
        }

        return Resultado.Sucesso();
    }

    protected abstract Result<T> CriarEntidade(Guid idUsuario, CategoriaRequest request);
    protected abstract Result<Unit> AtualizarEntidade(T categoria, CategoriaRequest request);
    protected abstract string ObterNome(T categoria);
    protected abstract Guid? ObterCategoriaPaiId(T categoria);
    protected abstract Guid ObterId(T categoria);
    protected abstract void DesativarEntidade(T categoria);

    protected bool PodeGerenciar(T categoria, Guid idUsuario, bool isAdmin)
        => ObterIdUsuario(categoria) == idUsuario || isAdmin;

    protected abstract Guid ObterIdUsuario(T categoria);

    protected async Task RegistrarHistoricoAsync(
        T categoria,
        EAcaoCategoriaHistorico acao,
        string? nomeAntigo = null,
        string? nomeNovo = null,
        Guid? categoriaPaiIdAntiga = null,
        Guid? categoriaPaiIdNova = null)
    {
        var historico = CategoriaHistorico.Criar(ObterId(categoria), TipoCategoria, ObterIdUsuario(categoria), acao, nomeAntigo, nomeNovo, categoriaPaiIdAntiga, categoriaPaiIdNova);
        await HistoricoRepository.InserirAsync(historico);
    }

    protected static CategoriaResponse Mapear(T c, Guid idUsuario, bool isAdmin, Func<T, Guid> obterId, Func<T, Guid> obterIdUsuario, Func<T, string> obterNome, Func<T, Guid?> obterCategoriaPaiId, Func<T, bool> obterAtivo, Func<T, DateTime> obterDataCadastro)
        => new()
        {
            Id = obterId(c),
            IdUsuario = obterIdUsuario(c),
            Nome = obterNome(c),
            CategoriaPaiId = obterCategoriaPaiId(c),
            Ativo = obterAtivo(c),
            PodeEditar = obterIdUsuario(c) == idUsuario || isAdmin,
            DataCadastro = obterDataCadastro(c)
        };

    protected abstract CategoriaResponse Mapear(T c, Guid idUsuario, bool isAdmin);
}
