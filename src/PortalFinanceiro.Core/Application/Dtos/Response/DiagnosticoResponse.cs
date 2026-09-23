namespace PortalFinanceiro.Core.Application.Dtos.Response;

public class DiagnosticoCenarioResponse
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Esperado { get; set; } = string.Empty;
    public bool Passou { get; set; }
    public string Detalhe { get; set; } = string.Empty;
}

public class DiagnosticoRegraResponse
{
    public string Id { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Fonte { get; set; } = string.Empty;
    public string Cobertura { get; set; } = string.Empty;
    public bool Passou { get; set; }
    public List<DiagnosticoCenarioResponse> Cenarios { get; set; } = new();
}

public class DiagnosticoBancoResponse
{
    public bool Passou { get; set; }
    public int Contratos { get; set; }
    public int ContratosRecorrentes { get; set; }
    public int CategoriasReceita { get; set; }
    public int Contas { get; set; }
    public string Detalhe { get; set; } = string.Empty;
}

public class DiagnosticoDebitoResponse
{
    public string Titulo { get; set; } = string.Empty;
    public string Onde { get; set; } = string.Empty;
    public string Impacto { get; set; } = string.Empty;
}

public class DiagnosticoResponse
{
    public DateTime GeradoEm { get; set; }
    public bool LiberadoParaMain { get; set; }
    public List<DiagnosticoRegraResponse> Regras { get; set; } = new();
    public DiagnosticoBancoResponse Banco { get; set; } = new();
    public List<DiagnosticoDebitoResponse> Debitos { get; set; } = new();
    public string ComandoSuite { get; set; } = string.Empty;
}
