export interface Parceria {
  id: string;
  nome: string;
  idParceiro: string;
  parceiro: string;
  idCliente: string;
  cliente: string;
  valor: number;
  percentualParceiro: number;
  valorParceiro: number;
  minhaParte: number;
  ativo: boolean;
  dataCadastro: string;
  totalRecebido: number;
  totalPago: number;
  faltaReceber: number;
  faltaPagar: number;
}

export interface ParceriaRequest {
  nome: string;
  idParceiro: string;
  idCliente: string;
  valor: number;
  percentualParceiro: number;
}
