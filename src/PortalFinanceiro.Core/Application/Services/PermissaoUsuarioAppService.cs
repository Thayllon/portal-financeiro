using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Services;

public class PermissaoUsuarioAppService : IPermissaoUsuarioAppService
{
    private readonly IPermissaoUsuarioRepository _repository;

    public PermissaoUsuarioAppService(IPermissaoUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<PermissaoUsuarioResponse>>> ListarPorUsuarioAsync(Guid usuarioId)
    {
        var permissoes = await _repository.ObterPorUsuarioIdAsync(usuarioId);
        return permissoes.Select(p => new PermissaoUsuarioResponse
        {
            Modulo = p.Modulo,
            Nivel = (int)p.Nivel
        }).ToList();
    }

    public async Task<Result<Unit>> SalvarPermissoesAsync(Guid usuarioId, IEnumerable<PermissaoUsuarioRequest> permissoes)
    {
        var existentes = (await _repository.ObterPorUsuarioIdAsync(usuarioId)).ToList();

        foreach (var req in permissoes)
        {
            var existente = existentes.FirstOrDefault(e => e.Modulo == req.Modulo);
            var nivel = (NivelPermissao)req.Nivel;

            if (existente is not null)
            {
                if (existente.Nivel != nivel)
                {
                    existente.AtualizarNivel(nivel);
                    await _repository.AtualizarAsync(existente);
                }
            }
            else
            {
                var nova = PermissaoUsuario.Criar(usuarioId, req.Modulo, nivel);
                await _repository.InserirAsync(nova);
            }
        }

        return Resultado.Sucesso();
    }

    public async Task<Result<bool>> VerificarPermissaoAsync(Guid usuarioId, string modulo, NivelPermissao nivelMinimo)
    {
        var permissao = await _repository.ObterPorUsuarioEModuloAsync(usuarioId, modulo);
        if (permissao is null)
            return false;

        return permissao.Nivel >= nivelMinimo;
    }
}
