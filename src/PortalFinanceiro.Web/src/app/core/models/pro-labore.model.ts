export interface ProLabore {
  id: string;
  ano: number;
  mes: number;
  valor: number;
  percentualInss: number;
  idConta: string;
  conta: string;
  ativo: boolean;
  dataCadastro: string;
}

export interface ProLaboreRequest {
  ano: number;
  mes: number;
  valor: number;
  percentualInss: number;
  idConta: string;
}
