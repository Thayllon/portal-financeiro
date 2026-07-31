export interface Recorrente {
  id: string;
  descricao: string;
  valor: number;
  dia: number;
  diaUtil: boolean;
  idCategoria: string;
  categoria: string;
  idConta: string;
  conta: string;
  dataInicio: string;
  dataFim?: string;
  ativo: boolean;
  dataCadastro: string;
}

export interface RecorrenteRequest {
  descricao: string;
  valor: number;
  dia: number;
  diaUtil: boolean;
  idCategoria: string;
  idConta: string;
  dataInicio: string;
  dataFim?: string;
}
