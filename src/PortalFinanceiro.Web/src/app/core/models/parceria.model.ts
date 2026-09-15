export interface Parceria {
  id: string;
  idParceiro: string;
  parceiro: string;
  idCliente: string;
  cliente: string;
  valor: number;
  ativo: boolean;
  dataCadastro: string;
  totalRecebido: number;
  totalPago: number;
  faltaReceber: number;
  faltaPagar: number;
}

export interface ParceriaRequest {
  idParceiro: string;
  idCliente: string;
  valor: number;
}
