using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Application.Validations;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Services;

namespace PortalFinanceiro.Core.Application.Services;

public class DiagnosticoAppService : IDiagnosticoAppService
{
    private readonly IContratoRepository _contratoRepository;
    private readonly ICategoriaReceitaRepository _categoriaRepository;
    private readonly IContaBancariaRepository _contaRepository;

    public DiagnosticoAppService(
        IContratoRepository contratoRepository,
        ICategoriaReceitaRepository categoriaRepository,
        IContaBancariaRepository contaRepository)
    {
        _contratoRepository = contratoRepository;
        _categoriaRepository = categoriaRepository;
        _contaRepository = contaRepository;
    }

    public async Task<DiagnosticoResponse> GerarAsync(Guid idUsuario)
    {
        var regras = new List<DiagnosticoRegraResponse>
        {
            AvaliarContratoSimples(),
            AvaliarContratoRecorrente(),
            AvaliarVinculoReceita(),
            AvaliarParceria(),
            AvaliarCategoria(),
            AvaliarDashboard()
        };
        var banco = await AvaliarBancoAsync(idUsuario);
        var response = new DiagnosticoResponse
        {
            GeradoEm = DateTime.UtcNow,
            Regras = regras,
            Banco = banco,
            Debitos = ListarDebitos(),
            ComandoSuite = "dotnet test PortalFinanceiro.API.slnx"
        };
        response.LiberadoParaMain = regras.All(r => r.Passou) && banco.Passou;
        return response;
    }

    private static DiagnosticoRegraResponse AvaliarContratoSimples()
    {
        var cenarios = new List<DiagnosticoCenarioResponse>();
        var idUsuario = Guid.NewGuid();
        var idCliente = Guid.NewGuid();

        var semNome = Contrato.Criar(idUsuario, "", idCliente, 100);
        cenarios.Add(Cenario("C1.1", "Criar sem nome é rejeitado", "NOME_OBRIGATORIO",
            !semNome.EhSucesso && semNome.Erro!.Codigo == "NOME_OBRIGATORIO", semNome.Erro?.Codigo ?? "aceito"));

        var valorZero = Contrato.Criar(idUsuario, "Aluguel", idCliente, 0);
        cenarios.Add(Cenario("C1.2", "Criar com valor zero é rejeitado", "VALOR_INVALIDO",
            !valorZero.EhSucesso && valorZero.Erro!.Codigo == "VALOR_INVALIDO", valorZero.Erro?.Codigo ?? "aceito"));

        var valido = Contrato.Criar(idUsuario, "Aluguel", idCliente, 100);
        cenarios.Add(Cenario("C1.3", "Criar válido nasce ativo e avulso", "Ativo=true, EhRecorrente=false",
            valido.EhSucesso && valido.Dado!.Ativo && !valido.Dado.EhRecorrente,
            valido.EhSucesso ? $"Ativo={valido.Dado!.Ativo}, EhRecorrente={valido.Dado.EhRecorrente}" : valido.Erro!.Codigo));

        return Regra("R1", "Contrato simples", "doc/regras.md#r1, Contrato.cs:20, ContratoAppService.cs:144/178", "parcial", cenarios);
    }

    private static DiagnosticoRegraResponse AvaliarContratoRecorrente()
    {
        var cenarios = new List<DiagnosticoCenarioResponse>();
        var validator = new ContratoRequestValidator();

        ContratoRequest RecorrenteValido() => new()
        {
            Nome = "Mensalidade",
            IdCliente = Guid.NewGuid(),
            Valor = 500,
            EhRecorrente = true,
            IdCategoria = Guid.NewGuid(),
            IdConta = Guid.NewGuid(),
            Dia = 5,
            DiaUtil = false,
            DataInicio = new DateTime(2026, 1, 1),
            DataFim = new DateTime(2026, 12, 31)
        };

        var semCategoria = RecorrenteValido();
        semCategoria.IdCategoria = null;
        var r1 = validator.Validate(semCategoria);
        cenarios.Add(Cenario("C2.1", "Recorrente sem categoria é rejeitado", "inválido",
            !r1.IsValid, r1.IsValid ? "aceito" : r1.Errors[0].ErrorMessage));

        var semConta = RecorrenteValido();
        semConta.IdConta = null;
        var r2 = validator.Validate(semConta);
        cenarios.Add(Cenario("C2.2", "Recorrente sem conta é rejeitado", "inválido",
            !r2.IsValid, r2.IsValid ? "aceito" : r2.Errors[0].ErrorMessage));

        var diaInvalido = RecorrenteValido();
        diaInvalido.Dia = 32;
        var r3 = validator.Validate(diaInvalido);
        cenarios.Add(Cenario("C2.3", "Dia 32 é rejeitado", "inválido",
            !r3.IsValid, r3.IsValid ? "aceito" : r3.Errors[0].ErrorMessage));

        var diaUtilInvalido = RecorrenteValido();
        diaUtilInvalido.DiaUtil = true;
        diaUtilInvalido.Dia = 6;
        var r4 = validator.Validate(diaUtilInvalido);
        cenarios.Add(Cenario("C2.4", "Dia útil com dia 6 é rejeitado", "inválido",
            !r4.IsValid, r4.IsValid ? "aceito" : r4.Errors[0].ErrorMessage));

        var periodoInvalido = RecorrenteValido();
        periodoInvalido.DataFim = new DateTime(2025, 12, 31);
        var r5 = validator.Validate(periodoInvalido);
        cenarios.Add(Cenario("C2.5", "Data fim anterior ao início é rejeitada", "inválido",
            !r5.IsValid, r5.IsValid ? "aceito" : r5.Errors[0].ErrorMessage));

        var valido = RecorrenteValido();
        var r6 = validator.Validate(valido);
        cenarios.Add(Cenario("C2.6", "Recorrente válido passa na validação", "válido",
            r6.IsValid, r6.IsValid ? "válido" : r6.Errors[0].ErrorMessage));

        var entidade = Contrato.Criar(Guid.NewGuid(), "Mensalidade", Guid.NewGuid(), 500, ehRecorrente: true);
        var regraId = Guid.NewGuid();
        if (entidade.EhSucesso) entidade.Dado!.VincularRegra(regraId);
        cenarios.Add(Cenario("C2.7", "VincularRegra marca EhRecorrente e guarda IdRegra", "EhRecorrente=true",
            entidade.EhSucesso && entidade.Dado!.EhRecorrente && entidade.Dado.IdRegra == regraId,
            entidade.EhSucesso ? $"EhRecorrente={entidade.Dado!.EhRecorrente}" : entidade.Erro!.Codigo));

        return Regra("R2", "Contrato recorrente", "doc/regras.md#r2, ContratoRequestValidator.cs:14, ContratoAppService.cs:58", "parcial", cenarios);
    }

    private static DiagnosticoRegraResponse AvaliarVinculoReceita()
    {
        var cenarios = new List<DiagnosticoCenarioResponse>();
        var validator = new ReceitaRequestValidator();

        ReceitaRequest Base() => new()
        {
            Descricao = "Serviço",
            Valor = 100,
            Data = new DateTime(2026, 1, 5),
            IdConta = Guid.NewGuid(),
            IdCategoria = Guid.NewGuid()
        };

        var duplo = Base();
        duplo.IdParceria = Guid.NewGuid();
        duplo.IdContrato = Guid.NewGuid();
        var r1 = validator.Validate(duplo);
        cenarios.Add(Cenario("C3.1", "Parceria + contrato juntos é rejeitado", "RECEITA_VINCULO_DUPLO",
            !r1.IsValid && r1.Errors.Any(e => e.ErrorMessage == "Informe parceria ou contrato, nunca os dois."),
            r1.IsValid ? "aceito" : r1.Errors[0].ErrorMessage));

        var soParceria = Base();
        soParceria.IdParceria = Guid.NewGuid();
        var r2 = validator.Validate(soParceria);
        cenarios.Add(Cenario("C3.2", "Só parceria passa", "válido", r2.IsValid, r2.IsValid ? "válido" : r2.Errors[0].ErrorMessage));

        var soContrato = Base();
        soContrato.IdContrato = Guid.NewGuid();
        var r3 = validator.Validate(soContrato);
        cenarios.Add(Cenario("C3.3", "Só contrato passa", "válido", r3.IsValid, r3.IsValid ? "válido" : r3.Errors[0].ErrorMessage));

        var semVinculo = Base();
        var r4 = validator.Validate(semVinculo);
        cenarios.Add(Cenario("C3.4", "Sem vínculo passa", "válido", r4.IsValid, r4.IsValid ? "válido" : r4.Errors[0].ErrorMessage));

        return Regra("R3", "Receita: um vínculo por vez", "doc/regras.md#r3, ReceitaRequestValidator.cs:16", "total", cenarios);
    }

    private static DiagnosticoRegraResponse AvaliarParceria()
    {
        var cenarios = new List<DiagnosticoCenarioResponse>();

        var percentualAlto = Parceria.Criar(Guid.NewGuid(), "Obra", Guid.NewGuid(), Guid.NewGuid(), 10000, 150);
        cenarios.Add(Cenario("C4.1", "Percentual 150 é rejeitado", "PERCENTUAL_INVALIDO",
            !percentualAlto.EhSucesso && percentualAlto.Erro!.Codigo == "PERCENTUAL_INVALIDO",
            percentualAlto.Erro?.Codigo ?? "aceito"));

        var valida = Parceria.Criar(Guid.NewGuid(), "Obra", Guid.NewGuid(), Guid.NewGuid(), 10000, 30);
        cenarios.Add(Cenario("C4.2", "Criar válida nasce ativa", "Ativo=true",
            valida.EhSucesso && valida.Dado!.Ativo, valida.EhSucesso ? "Ativo=true" : valida.Erro!.Codigo));

        return Regra("R4", "Parceria", "doc/regras.md#r4, Parceria.cs:20, ParceriaAppService.cs:78", "parcial", cenarios);
    }

    private static DiagnosticoRegraResponse AvaliarCategoria()
    {
        var cenarios = new List<DiagnosticoCenarioResponse>();

        var semNome = CategoriaReceita.Criar(Guid.NewGuid(), "");
        cenarios.Add(Cenario("C5.1", "Criar sem nome é rejeitado", "NOME_OBRIGATORIO",
            !semNome.EhSucesso && semNome.Erro!.Codigo == "NOME_OBRIGATORIO", semNome.Erro?.Codigo ?? "aceito"));

        var valida = CategoriaReceita.Criar(Guid.NewGuid(), "Serviços");
        cenarios.Add(Cenario("C5.2", "Criar válida nasce ativa", "Ativo=true",
            valida.EhSucesso && valida.Dado!.Ativo, valida.EhSucesso ? "Ativo=true" : valida.Erro!.Codigo));

        return Regra("R5", "Categorias compartilhadas", "doc/regras.md#r5, CategoriaBaseAppService.cs", "parcial", cenarios);
    }

    private static DiagnosticoRegraResponse AvaliarDashboard()
    {
        var cenarios = new List<DiagnosticoCenarioResponse>();

        var meses = LancamentoHelper.GerarMeses(new DateTime(2026, 1, 1), new DateTime(2026, 3, 31));
        cenarios.Add(Cenario("C6.1", "Previsão jan–mar/2026 gera 3 meses", "3 meses",
            meses.Count == 3, $"{meses.Count} meses"));

        var vencimento = LancamentoHelper.CalcularDataVencimento(31, false, 2, 2026);
        cenarios.Add(Cenario("C6.2", "Dia 31 em fev/2026 cai no dia 28", "28/02/2026",
            vencimento == new DateTime(2026, 2, 28), vencimento.ToString("dd/MM/yyyy")));

        return Regra("R6", "Dashboard previsto", "doc/regras.md#r6, LancamentoHelper.cs, DashboardAppService.cs", "parcial", cenarios);
    }

    private async Task<DiagnosticoBancoResponse> AvaliarBancoAsync(Guid idUsuario)
    {
        try
        {
            var contratos = (await _contratoRepository.ListarAsync(idUsuario, null, null)).ToList();
            var categorias = (await _categoriaRepository.ListarAsync()).ToList();
            var contas = (await _contaRepository.ListarPorUsuarioAsync(idUsuario)).ToList();
            return new DiagnosticoBancoResponse
            {
                Passou = true,
                Contratos = contratos.Count,
                ContratosRecorrentes = contratos.Count(c => c.EhRecorrente),
                CategoriasReceita = categorias.Count,
                Contas = contas.Count,
                Detalhe = "Leitura de Contrato, CategoriaReceita e ContaBancaria concluída."
            };
        }
        catch (Exception ex)
        {
            return new DiagnosticoBancoResponse
            {
                Passou = false,
                Detalhe = ex.Message
            };
        }
    }

    private static List<DiagnosticoDebitoResponse> ListarDebitos()
    {
        return new List<DiagnosticoDebitoResponse>
        {
            new() { Titulo = "TransactionScope aninhado na criação de contrato recorrente", Onde = "ContratoAppService.cs:113 + ReceitaRepository.cs:InserirEmMassaAsync", Impacto = "Acoplamento transacional frágil entre contrato, regra e parcelas." },
            new() { Titulo = "Agregação do dashboard sem cenário automatizado", Onde = "doc/regras.md#r6", Impacto = "Matemática da previsão verificada (C6.1, C6.2); agregação por conta/categoria não." },
            new() { Titulo = "Regras de serviço com cobertura parcial no diagnóstico", Onde = "doc/regras.md (encerrar com pendência, posse/auditoria de categoria)", Impacto = "Caminhos que exigem banco só são cobertos pela suite xUnit." }
        };
    }

    private static DiagnosticoCenarioResponse Cenario(string id, string nome, string esperado, bool passou, string detalhe)
    {
        return new DiagnosticoCenarioResponse { Id = id, Nome = nome, Esperado = esperado, Passou = passou, Detalhe = detalhe };
    }

    private static DiagnosticoRegraResponse Regra(string id, string titulo, string fonte, string cobertura, List<DiagnosticoCenarioResponse> cenarios)
    {
        return new DiagnosticoRegraResponse
        {
            Id = id,
            Titulo = titulo,
            Fonte = fonte,
            Cobertura = cobertura,
            Cenarios = cenarios,
            Passou = cenarios.Count > 0 && cenarios.All(c => c.Passou)
        };
    }
}
