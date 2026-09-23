using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/diagnostico")]
[Authorize(Roles = "Admin")]
public class DiagnosticoController : BaseController
{
    private const string ModuloQa = "qa";

    private readonly IDiagnosticoAppService _service;
    private readonly IPermissaoUsuarioAppService _permissoes;

    public DiagnosticoController(IDiagnosticoAppService service, IPermissaoUsuarioAppService permissoes)
    {
        _service = service;
        _permissoes = permissoes;
    }

    [HttpGet]
    public async Task<IActionResult> Gerar()
    {
        var idUsuario = ObterIdUsuario();
        var liberado = await _permissoes.VerificarPermissaoAsync(idUsuario, ModuloQa, NivelPermissao.Leitura);
        if (!liberado.EhSucesso || !liberado.Dado)
            return StatusCode(403, Erro.Permissao("QA_ACESSO_NEGADO", "Testes e QA não liberado para este usuário."));

        var result = await _service.GerarAsync(idUsuario);
        return Ok(result);
    }
}
