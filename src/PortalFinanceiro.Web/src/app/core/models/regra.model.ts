export interface Regra {
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
  dataFim: string;
  ativo: boolean;
}

export interface RegraRequest {
  descricao: string;
  valor: number;
  dia: number;
  diaUtil: boolean;
  idCategoria: string;
  idConta: string;
  dataInicio: string;
  dataFim: string;
}
