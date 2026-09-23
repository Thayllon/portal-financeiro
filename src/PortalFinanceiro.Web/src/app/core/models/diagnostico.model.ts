export interface DiagnosticoCenario {
  id: string;
  nome: string;
  esperado: string;
  passou: boolean;
  detalhe: string;
}

export interface DiagnosticoRegra {
  id: string;
  titulo: string;
  fonte: string;
  cobertura: string;
  passou: boolean;
  cenarios: DiagnosticoCenario[];
}

export interface DiagnosticoBanco {
  passou: boolean;
  contratos: number;
  contratosRecorrentes: number;
  categoriasReceita: number;
  contas: number;
  detalhe: string;
}

export interface DiagnosticoDebito {
  titulo: string;
  onde: string;
  impacto: string;
}

export interface Diagnostico {
  geradoEm: string;
  liberadoParaMain: boolean;
  regras: DiagnosticoRegra[];
  banco: DiagnosticoBanco;
  debitos: DiagnosticoDebito[];
  comandoSuite: string;
}
