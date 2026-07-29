export interface Dashboard {
  mes: number;
  ano: number;
  totalReceitas: number;
  totalRecebido: number;
  totalDespesas: number;
  totalPago: number;
  saldo: number;
  saldoRealizado: number;
  resumoPorConta: ResumoPorConta[];
  previsaoProximosMeses: PrevisaoMensal[];
}

export interface ResumoPorConta {
  nomeConta: string;
  banco: string;
  tipo: string;
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}

export interface PrevisaoMensal {
  mes: number;
  ano: number;
  totalReceitas: number;
  totalDespesas: number;
  saldoPrevisto: number;
}
