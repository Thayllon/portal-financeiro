export interface Dashboard {
  mes: number;
  ano: number;
  totalReceitas: number;
  totalRecebido: number;
  totalDespesas: number;
  totalPago: number;
  saldo: number;
  saldoRealizado: number;
  totalReceitasPrevisto: number;
  totalDespesasPrevisto: number;
  totalReceitasRecorrentes: number;
  totalDespesasRecorrentes: number;
  saldoPrevisto: number;
  resumoParcerias: ResumoParceriasAnual;
  resumoPorConta: ResumoPorConta[];
  distribuicaoReceitas: DistribuicaoCategoriaAnual[];
  distribuicaoDespesas: DistribuicaoCategoriaAnual[];
  previsaoProximosMeses: PrevisaoMensal[];
}

export interface DashboardAnual {
  ano: number;
  totalReceitas: number;
  totalRecebido: number;
  totalDespesas: number;
  totalPago: number;
  saldo: number;
  saldoRealizado: number;
  variacaoReceitasPercentual: number | null;
  variacaoDespesasPercentual: number | null;
  variacaoSaldoPercentual: number | null;
  mediaMensalSaldo: number;
  mesesConsiderados: number;
  resumoPorMes: MensalResumoAnual[];
  resumoPorConta: ResumoPorContaAnual[];
  distribuicaoReceitas: DistribuicaoCategoriaAnual[];
  distribuicaoDespesas: DistribuicaoCategoriaAnual[];
  previsaoRestanteAno: PrevisaoMensal[];
  resumoParcerias: ResumoParceriasAnual;
}

export interface DistribuicaoCategoriaAnual {
  nome: string;
  total: number;
  percentual: number;
  subcategorias: DistribuicaoCategoriaAnual[];
}

export interface ResumoParceriasAnual {
  totalRecebido: number;
  totalPago: number;
  aReceber: number;
  aPagar: number;
  saldo: number;
  qtdParcerias: number;
}

export interface MensalResumoAnual {
  mes: number;
  totalReceitas: number;
  totalRecebido: number;
  totalDespesas: number;
  totalPago: number;
  saldo: number;
  saldoRealizado: number;
}

export interface ResumoPorContaAnual {
  nomeConta: string;
  banco: string;
  tipo: string;
  totalReceitas: number;
  totalRecebido: number;
  totalDespesas: number;
  totalPago: number;
  saldo: number;
  saldoRealizado: number;
}

export interface ResumoPorConta {
  nomeConta: string;
  banco: string;
  tipo: string;
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
  totalReceitasPrevisto: number;
  totalDespesasPrevisto: number;
}

export interface PrevisaoMensal {
  mes: number;
  ano: number;
  totalReceitas: number;
  totalDespesas: number;
  saldoPrevisto: number;
}
