export interface Contrato {
  id: string;
  nome: string;
  idCliente: string;
  cliente: string;
  valor: number;
  ativo: boolean;
  ehRecorrente: boolean;
  idRegra?: string;
  dataCadastro: string;
  totalRecebido: number;
  faltaReceber: number;
}

export interface ContratoRequest {
  nome: string;
  idCliente: string;
  valor: number;
  ehRecorrente?: boolean;
  idCategoria?: string;
  idSubcategoria?: string;
  idConta?: string;
  dia?: number;
  diaUtil?: boolean;
  dataInicio?: string;
  dataFim?: string;
}
