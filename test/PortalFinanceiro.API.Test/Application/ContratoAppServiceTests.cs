using FluentAssertions;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.API.Test;

public class ContratoAppServiceTests
{
    private sealed class ContratoRepositoryFake : IContratoRepository
    {
        public List<ContratoProjecao> Itens { get; } = new();
        public ContratoProjecao? PorId { get; set; }
        public Dictionary<Guid, Contrato> Entidades { get; } = new();
        public int ChamadasSomarReceitas { get; private set; }
        public decimal SomaReceitas { get; set; }

        public Task<Contrato?> ObterPorIdAsync(Guid id) => Task.FromResult(Entidades.GetValueOrDefault(id));
        public Task<ContratoProjecao?> ObterProjecaoPorIdAsync(Guid id) => Task.FromResult(PorId);
        public Task<IEnumerable<ContratoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null, bool? ehRecorrente = null) => throw new NotImplementedException();
        public Task<IEnumerable<ContratoProjecao>> ListarComTotaisAsync(Guid idUsuario, bool? ativo, bool? ehRecorrente, int statusRealizado)
            => Task.FromResult<IEnumerable<ContratoProjecao>>(Itens.ToList());
        public Task InserirAsync(Contrato entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Contrato entity) => throw new NotImplementedException();
        public Task ExcluirAsync(Guid id)
        {
            Entidades.Remove(id);
            return Task.CompletedTask;
        }
        public Task<decimal> SomarReceitasPorStatusAsync(Guid idContrato, int status)
        {
            ChamadasSomarReceitas++;
            return Task.FromResult(SomaReceitas);
        }
    }

    private sealed class ProcessoRepositoryFake : IProcessoRepository
    {
        public int ProcessosVinculados { get; set; }
        public int ProcessosAtivosVinculados { get; set; }

        public Task<Processo?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ProcessoProjecao?> ObterProjecaoPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ProcessoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null) => throw new NotImplementedException();
        public Task InserirAsync(Processo entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Processo entity) => throw new NotImplementedException();
        public Task ExcluirProcessoAsync(Guid id) => throw new NotImplementedException();
        public Task<int> ContarAtivosPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task<int> ContarAtivosPorContratoAsync(Guid idContrato) => Task.FromResult(ProcessosAtivosVinculados);
        public Task<int> ContarPorParceriaAsync(Guid idParceria) => throw new NotImplementedException();
        public Task<int> ContarPorContratoAsync(Guid idContrato) => Task.FromResult(ProcessosVinculados);
        public Task<ProcessoEtapa?> ObterEtapaPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ProcessoEtapa>> ListarEtapasAsync(Guid idProcesso) => throw new NotImplementedException();
        public Task<int> ProximaOrdemAsync(Guid idProcesso) => throw new NotImplementedException();
        public Task<int> ContarEtapasPendentesAsync(Guid idProcesso) => throw new NotImplementedException();
        public Task InserirEtapaAsync(ProcessoEtapa entity) => throw new NotImplementedException();
        public Task AtualizarEtapaAsync(ProcessoEtapa entity) => throw new NotImplementedException();
        public Task ExcluirEtapaAsync(Guid id) => throw new NotImplementedException();
        public Task<ProcessoEtapaItem?> ObterItemPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<ProcessoEtapaItem>> ListarItensAsync(Guid idProcessoEtapa) => throw new NotImplementedException();
        public Task<int> ProximaOrdemItemAsync(Guid idProcessoEtapa) => throw new NotImplementedException();
        public Task<int> ContarItensObrigatoriosPendentesAsync(Guid idProcessoEtapa) => throw new NotImplementedException();
        public Task<int> ContarAnexosPorItemAsync(Guid idProcessoEtapaItem) => throw new NotImplementedException();
        public Task InserirItemAsync(ProcessoEtapaItem entity) => throw new NotImplementedException();
        public Task AtualizarItemAsync(ProcessoEtapaItem entity) => throw new NotImplementedException();
        public Task ExcluirItemAsync(Guid id) => throw new NotImplementedException();
    }

    private sealed class PessoaRepositoryFake : IPessoaRepository
    {
        public Task<Pessoa?> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<Pessoa>> ListarPorUsuarioAsync(Guid idUsuario) => throw new NotImplementedException();
        public Task InserirAsync(Pessoa entity) => throw new NotImplementedException();
        public Task AtualizarAsync(Pessoa entity) => throw new NotImplementedException();
    }

    private static ContratoProjecao NovaProjecao(Guid usuario, decimal totalRecebido = 0)
        => new()
        {
            Id = Guid.NewGuid(),
            IdUsuario = usuario,
            Nome = "Contrato",
            IdCliente = Guid.NewGuid(),
            Cliente = "Cliente",
            Valor = 12000,
            Ativo = true,
            EhRecorrente = false,
            DataCadastro = DateTime.UtcNow,
            TotalRecebido = totalRecebido
        };

    [Fact]
    public async Task Listar_UsaTotalDaProjecao_SemQueryPorItem()
    {
        var repos = new ContratoRepositoryFake();
        var usuario = Guid.NewGuid();
        repos.Itens.Add(NovaProjecao(usuario, totalRecebido: 3000));
        repos.Itens.Add(NovaProjecao(usuario, totalRecebido: 5000));
        var service = new ContratoAppService(repos, new PessoaRepositoryFake());

        var result = await service.ListarAsync(usuario);

        result.EhSucesso.Should().BeTrue();
        var lista = result.Dado!.ToList();
        lista.Should().HaveCount(2);
        lista[0].TotalRecebido.Should().Be(3000);
        lista[0].FaltaReceber.Should().Be(9000);
        lista[1].TotalRecebido.Should().Be(5000);
        lista[1].FaltaReceber.Should().Be(7000);
        repos.ChamadasSomarReceitas.Should().Be(0);
    }

    [Fact]
    public async Task ObterPorId_BuscaTotalIndividualmente()
    {
        var repos = new ContratoRepositoryFake();
        var usuario = Guid.NewGuid();
        repos.PorId = NovaProjecao(usuario);
        repos.SomaReceitas = 3000;
        var service = new ContratoAppService(repos, new PessoaRepositoryFake());

        var result = await service.ObterPorIdAsync(repos.PorId.Id, usuario);

        result.EhSucesso.Should().BeTrue();
        result.Dado!.TotalRecebido.Should().Be(3000);
        result.Dado!.FaltaReceber.Should().Be(9000);
        repos.ChamadasSomarReceitas.Should().Be(1);
    }

    private static Contrato NovoContrato(Guid usuario)
        => Contrato.Criar(usuario, "Contrato", Guid.NewGuid(), 12000).Dado!;

    [Fact]
    public async Task Excluir_SemVinculos_RemoveDefinitivamente()
    {
        var repos = new ContratoRepositoryFake();
        var processos = new ProcessoRepositoryFake();
        var usuario = Guid.NewGuid();
        var contrato = NovoContrato(usuario);
        repos.Entidades[contrato.Id] = contrato;
        var service = new ContratoAppService(repos, new PessoaRepositoryFake(), processoRepository: processos);

        var result = await service.ExcluirAsync(contrato.Id, usuario);

        result.EhSucesso.Should().BeTrue();
        repos.Entidades.Should().NotContainKey(contrato.Id);
    }

    [Fact]
    public async Task Excluir_ComReceitas_Bloqueia()
    {
        var repos = new ContratoRepositoryFake();
        var usuario = Guid.NewGuid();
        var contrato = NovoContrato(usuario);
        repos.Entidades[contrato.Id] = contrato;
        repos.SomaReceitas = 1000;
        var service = new ContratoAppService(repos, new PessoaRepositoryFake());

        var result = await service.ExcluirAsync(contrato.Id, usuario);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("CONTRATO_COM_VINCULOS");
        repos.Entidades.Should().ContainKey(contrato.Id);
    }

    [Fact]
    public async Task Excluir_ComProcessosEncerrados_Bloqueia()
    {
        var repos = new ContratoRepositoryFake();
        var processos = new ProcessoRepositoryFake { ProcessosVinculados = 1 };
        var usuario = Guid.NewGuid();
        var contrato = NovoContrato(usuario);
        repos.Entidades[contrato.Id] = contrato;
        var service = new ContratoAppService(repos, new PessoaRepositoryFake(), processoRepository: processos);

        var result = await service.ExcluirAsync(contrato.Id, usuario);

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("CONTRATO_COM_PROCESSOS");
        repos.Entidades.Should().ContainKey(contrato.Id);
    }

    [Fact]
    public async Task Excluir_DeOutroUsuario_RetornaPermissao()
    {
        var repos = new ContratoRepositoryFake();
        var contrato = NovoContrato(Guid.NewGuid());
        repos.Entidades[contrato.Id] = contrato;
        var service = new ContratoAppService(repos, new PessoaRepositoryFake());

        var result = await service.ExcluirAsync(contrato.Id, Guid.NewGuid());

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("CONTRATO_ACESSO_NEGADO");
        repos.Entidades.Should().ContainKey(contrato.Id);
    }

    [Fact]
    public async Task Excluir_Inexistente_RetornaNaoEncontrado()
    {
        var service = new ContratoAppService(new ContratoRepositoryFake(), new PessoaRepositoryFake());

        var result = await service.ExcluirAsync(Guid.NewGuid(), Guid.NewGuid());

        result.EhSucesso.Should().BeFalse();
        result.Erro!.Codigo.Should().Be("NAO_ENCONTRADO");
    }
}