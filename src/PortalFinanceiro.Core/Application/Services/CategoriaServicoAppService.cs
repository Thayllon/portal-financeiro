using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;
using Microsoft.Extensions.Logging;

namespace PortalFinanceiro.Core.Application.Services;

public class CategoriaServicoAppService : CategoriaBaseAppService<CategoriaServico>, ICategoriaServicoAppService
{
    public CategoriaServicoAppService(
        ICategoriaServicoRepository repository,
        ICategoriaHistoricoRepository historicoRepository,
        ILogger<CategoriaServicoAppService>? logger = null)
        : base(repository, historicoRepository, ETipoCategoria.Servicos, logger) { }

    public async Task<Result<Unit>> ExcluirAsync(Guid id, Guid idUsuario, bool isAdmin)
        => await base.ExcluirAsync(id, idUsuario, isAdmin, _ => Task.FromResult(0));

    protected override Result<CategoriaServico> CriarEntidade(Guid idUsuario, CategoriaRequest request)
        => CategoriaServico.Criar(idUsuario, request.Nome, request.CategoriaPaiId);

    protected override Result<Unit> AtualizarEntidade(CategoriaServico categoria, CategoriaRequest request)
        => categoria.Atualizar(request.Nome, request.CategoriaPaiId);

    protected override string ObterNome(CategoriaServico categoria) => categoria.Nome;
    protected override Guid? ObterCategoriaPaiId(CategoriaServico categoria) => categoria.CategoriaPaiId;
    protected override Guid ObterId(CategoriaServico categoria) => categoria.Id;
    protected override void DesativarEntidade(CategoriaServico categoria) => categoria.Desativar();
    protected override Guid ObterIdUsuario(CategoriaServico categoria) => categoria.IdUsuario;
    protected override void DefinirCriadorEntidade(CategoriaServico categoria, Guid ator) => categoria.DefinirCriador(ator);
    protected override void DefinirEditorEntidade(CategoriaServico categoria, Guid ator) => categoria.DefinirEditor(ator);

    protected override CategoriaResponse Mapear(CategoriaServico c, Guid idUsuario, bool isAdmin)
        => CategoriaBaseAppService<CategoriaServico>.Mapear(c, idUsuario, isAdmin, x => x.Id, x => x.IdUsuario, x => x.Nome, x => x.CategoriaPaiId, x => x.Ativo, x => x.DataCadastro, x => x.CriadoPor, x => x.AlteradoPor);
}
