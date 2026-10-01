using FluentAssertions;
using PortalFinanceiro.Core.Application.Dtos.Request;
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
        public Dictionary<Guid, ProcessoEtapaItem> Itens { get; } = new();

        public Task<Processo?> ObterPorIdAsync(Guid id)
            => Task.FromResult(Processos.GetValueOrDefault(id));
        public Task<ProcessoProjecao?> ObterProjecaoPorIdAsync(Guid id)
        {
            if (!Processos.TryGetValue(id, out var p))
                return Task.FromResult<ProcessoProjecao?>(null);
            var etapas = Etapas.Values.Where(e => e.IdProcesso == id).ToList();
            var itens = Itens.Values.Where(i => etapas.Any(e => e.Id == i.IdProcessoEtapa)).ToList();
            return Task.FromResult<ProcessoProjecao?>(new ProcessoProjecao
            {
                Id = p.Id,
                IdUsuario = p.IdUsuario,
                Nome = p.Nome,
                Descricao = p.Descricao,
                IdModeloProcesso = p.IdModeloProcesso,
                IdCliente = p.IdCliente,
                Ativo = p.Ativo,
                DataEncerramento = p.DataEncerramento,
                DataCadastro = p.DataCadastro,
                TotalEtapas = etapas.Count,
                EtapasConcluidas = etapas.Count(e => e.Concluida),
                TotalItens = itens.Count,
                ItensConcluidos = itens.Count(i => i.Concluida)
            });
        }
        public Task<IEnumerable<ProcessoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null) => throw new NotImplementedException();
        public Task InserirAsync(Processo entity)
        {
            Processos[entity.Id] = entity;
            return Task.CompletedTask;
        }
        public Task AtualizarAsync(Processo entity)
        {
            Processos[entity.Id] = entity;
            return Task.CompletedTask;
        }
        public Task ExcluirProcessoAsync(Guid id)
        {
            Processos.Remove(id);
            return Task.CompletedTask;
        }
        public Task<int> ContarAtivosPorParceriaAsync(Guid idParceria) => Task.FromResult(0);
        public Task<int> ContarAtivosPorContratoAsync(Guid idContrato) => Task.FromResult(0);
        public Task<int> ContarPorParceriaAsync(Guid idParceria) => Task.FromResult(0);
        public Task<int> ContarPorContratoAsync(Guid idContrato) => Task.FromResult(0);
        public Task<ProcessoEtapa?> ObterEtapaPorIdAsync(Guid id)
            => Task.FromResult(Etapas.GetValueOrDefault(id));
        public Task<IEnumerable<ProcessoEtapa>> ListarEtapasAsync(Guid idProcesso)
            => Task.FromResult<IEnumerable<ProcessoEtapa>>(Etapas.Values.Where(e => e.IdProcesso == idProcesso).ToList());
        public Task<int> ProximaOrdemAsync(Guid idProcesso) => throw new NotImplementedException();
        public Task<int> ContarEtapasPendentesAsync(Guid idProcesso) => throw new NotImplementedException();
        public Task InserirEtapaAsync(ProcessoEtapa entity)
        {
            Etapas[entity.Id] = entity;
            return Task.CompletedTask;
        }
        public Task AtualizarEtapaAsync(ProcessoEtapa entity)
        {
            Etapas[entity.Id] = entity;
            return Task.CompletedTask;
        }
        public Task ExcluirEtapaAsync(Guid id)
        {
            Etapas.Remove(id);
            return Task.CompletedTask;
        }
        public Task<ProcessoEtapaItem?> ObterItemPorIdAsync(Guid id)
            => Task.FromResult(Itens.GetValueOrDefault(id));
        public Task<IEnumerable<ProcessoEtapaItem>> ListarItensAsync(Guid idProcessoEtapa)
            => Task.FromResult<IEnumerable<ProcessoEtapaItem>>(Itens.Values.Where(i => i.IdProcessoEtapa == idProcessoEtapa).ToList());
        public Task<int> ProximaOrdemItemAsync(Guid idProcessoEtapa)
            => Task.FromResult(Itens.Values.Where(i => i.IdProcessoEtapa == idProcessoEtapa).Select(i => i.Ordem).DefaultIfEmpty(0).Max() + 1);
        public Task<int> ContarItensObrigatoriosPendentesAsync(Guid idProcessoEtapa)
            => Task.FromResult(Itens.Values.Count(i => i.IdProcessoEtapa == idProcessoEtapa && i.Obrigatorio && !i.Concluida));
        public Task<int> ContarAnexosPorItemAsync(Guid idProcessoEtapaItem) => Task.FromResult(0);
        public Task InserirItemAsync(ProcessoEtapaItem entity)
        {
            Itens[entity.Id] = entity;
            return Task.CompletedTask;
        }
        public Task AtualizarItemAsync(ProcessoEtapaItem entity)
        {
            Itens[entity.Id] = entity;
            return Task.CompletedTask;
        }
        public Task ExcluirItemAsync(Guid id)
        {
            Itens.Remove(id);
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
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
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
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<decimal> SomarReceitasPorStatusAsync(Guid idContrato, int status) => throw new NotImplementedException();
    }

    private sealed class ModeloProcessoRepositoryFake : IModeloProcessoRepository
    {
        public Dictionary<Guid, ModeloProcesso> Modelos { get; } = new();
        public Dictionary<Guid, ModeloEtapa> Etapas { get; } = new();
        public Dictionary<Guid, ModeloItem> Itens { get; } = new();

        public Task<ModeloProcesso?> ObterPorIdAsync(Guid id)
            => Task.FromResult(Modelos.GetValueOrDefault(id));
        public Task<ModeloProcessoProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ModeloProcessoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null) => throw new NotImplementedException();
        public Task InserirAsync(ModeloProcesso entity) => throw new NotImplementedException();
        public Task AtualizarAsync(ModeloProcesso entity) => throw new NotImplementedException();
        public Task ExcluirAsync(Guid id) => throw new NotImplementedException();
        public Task<ModeloEtapa?> ObterEtapaPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ModeloEtapa>> ListarEtapasAsync(Guid idModeloProcesso)
            => Task.FromResult<IEnumerable<ModeloEtapa>>(Etapas.Values.Where(e => e.IdModeloProcesso == idModeloProcesso).ToList());
        public Task<int> ProximaOrdemEtapaAsync(Guid idModeloProcesso) => throw new NotImplementedException();
        public Task InserirEtapaAsync(ModeloEtapa entity) => throw new NotImplementedException();
        public Task AtualizarEtapaAsync(ModeloEtapa entity) => throw new NotImplementedException();
        public Task ExcluirEtapaAsync(Guid id) => throw new NotImplementedException();
        public Task<ModeloItem?> ObterItemPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ModeloItem>> ListarItensAsync(Guid idModeloEtapa)
            => Task.FromResult<IEnumerable<ModeloItem>>(Itens.Values.Where(i => i.IdModeloEtapa == idModeloEtapa).ToList());
        public Task<int> ProximaOrdemItemAsync(Guid idModeloEtapa) => throw new NotImplementedException();
        public Task InserirItemAsync(ModeloItem entity) => throw new NotImplementedException();
        public Task AtualizarItemAsync(ModeloItem entity) => throw new NotImplementedException();
        public Task ExcluirItemAsync(Guid id) => throw new NotImplementedException();
    }

    private sealed class PessoaRepositoryFake : IPessoaRepository
    {
        public Task<Pessoa?> ObterPorIdAsync(Guid id) => Task.FromResult<Pessoa?>(null);
        public Task<IEnumerable<Pessoa>> ListarPorUsuarioAsync(Guid idUsuario) => throw new NotImplementedException();
        public Task InserirAsync(Pessoa entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Pessoa entity) => throw new NotImplementedException();
    }

    [Fact]
    public async Task ExcluirAsync_RemoveProcessoEQuestoes()
    {
        var usuario = Guid.NewGuid();
        var repo = new ProcessoRepositoryFake();
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake(), new ModeloProcessoRepositoryFake(), new PessoaRepositoryFake());
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
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake(), new ModeloProcessoRepositoryFake(), new PessoaRepositoryFake());
        var processo = Processo.Criar(Guid.NewGuid(), "Regularizar casa", null, Guid.NewGuid(), null).Dado!;
        repo.Processos[processo.Id] = processo;

        var result = await service.ExcluirAsync(processo.Id, Guid.NewGuid());

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("PROCESSO_ACESSO_NEGADO");
        repo.Processos.Should().ContainKey(processo.Id);
    }

    [Fact]
    public async Task AdicionarAsync_ComModelo_InstanciaEtapasEItens()
    {
        var usuario = Guid.NewGuid();
        var repo = new ProcessoRepositoryFake();
        var modelos = new ModeloProcessoRepositoryFake();
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake(), modelos, new PessoaRepositoryFake());

        var modelo = ModeloProcesso.Criar(usuario, "Regularização", null).Dado!;
        var fase = ModeloEtapa.Criar(modelo.Id, "Documental", null, 1).Dado!;
        var item1 = ModeloItem.Criar(fase.Id, "Orçamento", null, true, false, 1).Dado!;
        var item2 = ModeloItem.Criar(fase.Id, "Contrato", null, true, false, 2).Dado!;
        modelos.Modelos[modelo.Id] = modelo;
        modelos.Etapas[fase.Id] = fase;
        modelos.Itens[item1.Id] = item1;
        modelos.Itens[item2.Id] = item2;

        var result = await service.AdicionarAsync(usuario, new ProcessoRequest { Nome = "Casa do cliente X", IdModeloProcesso = modelo.Id });

        result.EhSucesso.Should().BeTrue();
        result.Dado!.IdModeloProcesso.Should().Be(modelo.Id);
        result.Dado!.Etapas.Should().HaveCount(1);
        result.Dado!.Etapas[0].Itens.Should().HaveCount(2);
        result.Dado!.Etapas[0].DataInicio.Should().NotBeNull();
        result.Dado!.Etapas[0].Itens[0].DataInicio.Should().NotBeNull();
        result.Dado!.Etapas[0].Itens[1].DataInicio.Should().BeNull();
    }

    [Fact]
    public async Task ConcluirEtapaAsync_ComItemObrigatorioPendente_RetornaNegocio()
    {
        var usuario = Guid.NewGuid();
        var repo = new ProcessoRepositoryFake();
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake(), new ModeloProcessoRepositoryFake(), new PessoaRepositoryFake());
        var processo = Processo.Criar(usuario, "Casa X", null, null, null).Dado!;
        var etapa = ProcessoEtapa.Criar(processo.Id, "Documental", null, 1, null).Dado!;
        var item = ProcessoEtapaItem.Criar(etapa.Id, "Orçamento", null, true, false, 1).Dado!;
        repo.Processos[processo.Id] = processo;
        repo.Etapas[etapa.Id] = etapa;
        repo.Itens[item.Id] = item;

        var bloqueado = await service.ConcluirEtapaAsync(processo.Id, etapa.Id, usuario);

        bloqueado.EhSucesso.Should().BeFalse();
        bloqueado.Erro!.Codigo.Should().Be("ETAPA_COM_ITENS_PENDENTES");

        var forcado = await service.ConcluirEtapaAsync(processo.Id, etapa.Id, usuario, forcar: true);

        forcado.EhSucesso.Should().BeTrue();
        forcado.Dado!.Concluida.Should().BeTrue();
    }

    [Fact]
    public async Task ConcluirItemAsync_IniciaProximoItem()
    {
        var usuario = Guid.NewGuid();
        var repo = new ProcessoRepositoryFake();
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake(), new ModeloProcessoRepositoryFake(), new PessoaRepositoryFake());
        var processo = Processo.Criar(usuario, "Casa X", null, null, null).Dado!;
        var etapa = ProcessoEtapa.Criar(processo.Id, "Documental", null, 1, null).Dado!;
        var item1 = ProcessoEtapaItem.Criar(etapa.Id, "Orçamento", null, true, false, 1).Dado!;
        var item2 = ProcessoEtapaItem.Criar(etapa.Id, "Contrato", null, true, false, 2).Dado!;
        repo.Processos[processo.Id] = processo;
        repo.Etapas[etapa.Id] = etapa;
        repo.Itens[item1.Id] = item1;
        repo.Itens[item2.Id] = item2;

        var result = await service.ConcluirItemAsync(processo.Id, item1.Id, usuario);

        result.EhSucesso.Should().BeTrue();
        repo.Itens[item2.Id].DataInicio.Should().NotBeNull();
        repo.Itens[item2.Id].Concluida.Should().BeFalse();
    }

    [Fact]
    public async Task ConcluirEtapaAsync_UltimaEtapa_EncerraProcessoAutomaticamente()
    {
        var usuario = Guid.NewGuid();
        var repo = new ProcessoRepositoryFake();
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake(), new ModeloProcessoRepositoryFake(), new PessoaRepositoryFake());
        var processo = Processo.Criar(usuario, "Casa X", null, null, null).Dado!;
        var etapa = ProcessoEtapa.Criar(processo.Id, "Documental", null, 1, null).Dado!;
        repo.Processos[processo.Id] = processo;
        repo.Etapas[etapa.Id] = etapa;

        var result = await service.ConcluirEtapaAsync(processo.Id, etapa.Id, usuario, forcar: true);

        result.EhSucesso.Should().BeTrue();
        repo.Processos[processo.Id].Ativo.Should().BeFalse();
        repo.Processos[processo.Id].DataEncerramento.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterPorIdAsync_PercentualCalculadoPorItens()
    {
        var usuario = Guid.NewGuid();
        var repo = new ProcessoRepositoryFake();
        var service = new ProcessoAppService(repo, new ParceriaRepositoryFake(), new ContratoRepositoryFake(), new ModeloProcessoRepositoryFake(), new PessoaRepositoryFake());
        var processo = Processo.Criar(usuario, "Casa X", null, null, null).Dado!;
        var etapa = ProcessoEtapa.Criar(processo.Id, "Documental", null, 1, null).Dado!;
        repo.Processos[processo.Id] = processo;
        repo.Etapas[etapa.Id] = etapa;
        for (var i = 1; i <= 4; i++)
        {
            var item = ProcessoEtapaItem.Criar(etapa.Id, $"Item {i}", null, true, false, i).Dado!;
            if (i == 1)
                item.MarcarConcluida();
            repo.Itens[item.Id] = item;
        }

        var result = await service.ObterPorIdAsync(processo.Id, usuario);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.TotalItens.Should().Be(4);
        result.Dado!.ItensConcluidos.Should().Be(1);
        result.Dado!.PercentualConcluido.Should().Be(25);
    }
}
