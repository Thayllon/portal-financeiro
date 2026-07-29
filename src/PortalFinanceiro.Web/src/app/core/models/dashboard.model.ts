export interface Dashboard {
  mes: number;
  ano: number;
  totalReceitas: number;
  totalRecebido: number;
  totalDespesas: number;
  totalPago: number;
  saldo: number;
  saldoRealizado: number;
  previsaoProximosMeses: PrevisaoMensal[];
}

export interface PrevisaoMensal {
  mes: number;
  ano: number;
  totalReceitas: number;
  totalDespesas: number;
  saldoPrevisto: number;
}
