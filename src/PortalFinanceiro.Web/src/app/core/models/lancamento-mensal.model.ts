export interface LancamentoMensal {
  id: string;
  idRecorrente: string;
  descricao: string;
  mes: number;
  ano: number;
  valor: number;
  dataRealizacao?: string;
  status: string;
}

export interface StatusRequest {
  data: string;
}
