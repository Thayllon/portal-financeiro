using FluentAssertions;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.API.Test;

public class ProcessoAppServiceTests
{
    private sealed class ProcessoRepositoryFake : IProcessoRepository
    {
        public Dictionary<Guid, Processo> Processos { get; } = new();
        public Dictionary<Guid, ProcessoEtapa> Etapas { get; } = new();

        public Task<Processo?> ObterPorIdAsync(Guid id)
            => Task.FromResult(Processos.GetValueOrDefault(id));
        public Task<ProcessoProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ProcessoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null) => throw new NotImplementedException();
        public Task InserirAsync(Processo entity)
        {
            Processos[entity.Id] = entity;
            return Task.CompletedTask;
        }
        public Task AtualizarAsync(Processo entity) => throw new NotImplementedException();
        public Task ExcluirProcessoAsync(Guid id)
        {
            Processos.Remove(id);
            return Task.CompletedTask;
        }
        public Task<int> ContarAtivosPorParceriaAsync(Guid idParceria) => Task.FromResult(0);
        public Task<int> ContarAtivosPorContratoAsync(Guid idContrato) => Task.FromResult(0);
        public Task<ProcessoEtapa?> ObterEtapaPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ProcessoEtapa>> ListarEtapasAsync(Guid idProcesso)
            => Task.FromResult<IEnumerable<ProcessoEtapa>>(Etapas.Values.Where(e => e.IdProcesso == idProcesso).ToList());
        public Task<int> ProximaOrdemAsync(Guid idProcesso) => throw new NotImplementedException();
        public Task<int> ContarEtapasPendentesAsync(Guid idProcesso) => throw new NotImplementedException();
        public Task InserirEtapaAsync(ProcessoEtapa entity) => throw new NotImplementedException();
        public Task AtualizarEtapaAsync(ProcessoEtapa entity) => throw new NotImplementedException();
        public Task ExcluirEtapaAsync(Guid id)
        {
            Etapas.Remove(id);
            return Task.CompletedTask;
        }
    }

    private sealed class ParceriaRepositoryFake : IParceriaRepository
    {
        public Task<Parceria?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ParceriaProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ParceriaProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null) => throw new NotImplementedException();
        public Task<IEnumerable<ParceriaProjecao>> ListarComTotaisAsync(Guid idUsuario, bool? ativo, int statusRealizado) => throw new NotImplementedException();
        public Task InserirAsync(Parceria entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Parceria entity) => throw new NotImplementedException();
        public Task<decimal> SomarReceitasPorStatusAsync(Guid idParceria, int status) => throw new NotImplementedException();
        public Task<decimal> SomarDespesasPorStatusAsync(Guid idParceria, int status) => throw new NotImplementedException();
        public Task<ResumoParceriaAnual> ResumoAnualAsync(Guid idUsuario, int ano, Guid? idConta = null) => throw new NotImplementedException();
        public Task<ResumoParceriaAnual> ResumoMensalAsync(Guid idUsuario, int ano, int mes, Guid? idConta = null) => throw new NotImplementedException();
    }

    private sealed class ContratoRepositoryFake : IContratoRepository
    {
        public Task<Contrato?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ContratoProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ContratoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null, bool? ehRecorrente = null) => throw new NotImplementedException();
        public Task<IEnumerable<ContratoProjecao>> ListarComTotaisAsync(Guid idUsuario, bool? ativo, bool? ehRecorrente, int statusRealizado) => throw new NotImplementedException();
        public Task InserirAsync(Contrato entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Contrato entity) => throw new NotImplementedException();
        public Task<decimal> SomarReceitasPorStatusAsync(Guid idContrato, int status) => throw new NotImplementedException();
    }

    [Fact]
    public async Task ExcluirAsync_RemoveProcessoEQuestoes()
    {
        var usuario = Guid.NewGuid();
        var repo = new ProcessoRepositoryFake();
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake());
        var processo = Processo.Criar(usuario, "Regularizar casa", null, Guid.NewGuid(), null).Dado!;
        var etapa1 = ProcessoEtapa.Criar(processo.Id, "Cliente pagou", null, 1, null).Dado!;
        var etapa2 = ProcessoEtapa.Criar(processo.Id, "Matrícula", null, 2, null).Dado!;
        repo.Processos[processo.Id] = processo;
        repo.Etapas[etapa1.Id] = etapa1;
        repo.Etapas[etapa2.Id] = etapa2;

        var result = await service.ExcluirAsync(processo.Id, usuario);

        result.EhSucesso.Should().BeTrue();
        repo.Processos.Should().NotContainKey(processo.Id);
        repo.Etapas.Should().BeEmpty();
    }

    [Fact]
    public async Task ExcluirAsync_DeOutroUsuario_RetornaPermissao()
    {
        var repo = new ProcessoRepositoryFake();
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake());
        var processo = Processo.Criar(Guid.NewGuid(), "Regularizar casa", null, Guid.NewGuid(), null).Dado!;
        repo.Processos[processo.Id] = processo;

        var result = await service.ExcluirAsync(processo.Id, Guid.NewGuid());

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("PROCESSO_ACESSO_NEGADO");
        repo.Processos.Should().ContainKey(processo.Id);
    }
}
