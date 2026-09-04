using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class CategoriaDespesaAppService : CategoriaBaseAppService<CategoriaDespesa>, ICategoriaDespesaAppService
{
    private readonly IDespesaRepository _despesaRepository;

    public CategoriaDespesaAppService(
        ICategoriaDespesaRepository repository,
        IDespesaRepository despesaRepository,
        ICategoriaHistoricoRepository historicoRepository)
        : base(repository, historicoRepository, ETipoCategoria.Despesa)
    {
        _despesaRepository = despesaRepository;
    }

    public new async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario, bool isAdmin)
        => await base.ExcluirAsync(id, idUsuario, isAdmin, async categoriaId =>
        {
            var vinculadas = await _despesaRepository.ContarPorCategoriaAsync(categoriaId);
            var subVinculadas = await _despesaRepository.ContarPorSubcategoriaAsync(categoriaId);
            return vinculadas + subVinculadas;
        });

    protected override Result<CategoriaDespesa> CriarEntidade(Guid idUsuario, CategoriaRequest request)
        => CategoriaDespesa.Criar(idUsuario, request.Nome, request.CategoriaPaiId);

    protected override Result<Unit> AtualizarEntidade(CategoriaDespesa categoria, CategoriaRequest request)
        => categoria.Atualizar(request.Nome, request.CategoriaPaiId);

    protected override string ObterNome(CategoriaDespesa categoria) => categoria.Nome;
    protected override Guid? ObterCategoriaPaiId(CategoriaDespesa categoria) => categoria.CategoriaPaiId;
    protected override Guid ObterId(CategoriaDespesa categoria) => categoria.Id;
    protected override void DesativarEntidade(CategoriaDespesa categoria) => categoria.Desativar();
    protected override Guid ObterIdUsuario(CategoriaDespesa categoria) => categoria.IdUsuario;

    protected override CategoriaResponse Mapear(CategoriaDespesa c, Guid idUsuario, bool isAdmin)
        => CategoriaBaseAppService<CategoriaDespesa>.Mapear(c, idUsuario, isAdmin, x => x.Id, x => x.IdUsuario, x => x.Nome, x => x.CategoriaPaiId, x => x.Ativo, x => x.DataCadastro);
}
