using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class CategoriaReceitaAppService : CategoriaBaseAppService<CategoriaReceita>, ICategoriaReceitaAppService
{
    private readonly IReceitaRepository _receitaRepository;

    public CategoriaReceitaAppService(
        ICategoriaReceitaRepository repository,
        IReceitaRepository receitaRepository,
        ICategoriaHistoricoRepository historicoRepository)
        : base(repository, historicoRepository, ETipoCategoria.Receita)
    {
        _receitaRepository = receitaRepository;
    }

    public new async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario, bool isAdmin)
        => await base.ExcluirAsync(id, idUsuario, isAdmin, async categoriaId =>
        {
            var vinculadas = await _receitaRepository.ContarPorCategoriaAsync(categoriaId);
            var subVinculadas = await _receitaRepository.ContarPorSubcategoriaAsync(categoriaId);
            return vinculadas + subVinculadas;
        });

    protected override Result<CategoriaReceita> CriarEntidade(Guid idUsuario, CategoriaRequest request)
        => CategoriaReceita.Criar(idUsuario, request.Nome, request.CategoriaPaiId);

    protected override Result<Unit> AtualizarEntidade(CategoriaReceita categoria, CategoriaRequest request)
        => categoria.Atualizar(request.Nome, request.CategoriaPaiId);

    protected override string ObterNome(CategoriaReceita categoria) => categoria.Nome;
    protected override Guid? ObterCategoriaPaiId(CategoriaReceita categoria) => categoria.CategoriaPaiId;
    protected override Guid ObterId(CategoriaReceita categoria) => categoria.Id;
    protected override void DesativarEntidade(CategoriaReceita categoria) => categoria.Desativar();
    protected override Guid ObterIdUsuario(CategoriaReceita categoria) => categoria.IdUsuario;

    protected override CategoriaResponse Mapear(CategoriaReceita c, Guid idUsuario, bool isAdmin)
        => CategoriaBaseAppService<CategoriaReceita>.Mapear(c, idUsuario, isAdmin, x => x.Id, x => x.IdUsuario, x => x.Nome, x => x.CategoriaPaiId, x => x.Ativo, x => x.DataCadastro);
}
