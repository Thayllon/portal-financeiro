export interface Contrato {
  id: string;
  nome: string;
  idCliente: string;
  cliente: string;
  valor: number;
  ativo: boolean;
  dataCadastro: string;
  totalRecebido: number;
  faltaReceber: number;
}

export interface ContratoRequest {
  nome: string;
  idCliente: string;
  valor: number;
}
