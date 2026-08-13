export interface Receita {
  id: string;
  descricao: string;
  valor: number;
  data: string;
  idConta: string;
  conta: string;
  idCategoria: string;
  categoria: string;
  idSubcategoria?: string;
  subcategoria: string;
  status: number;
  dataRealizacao?: string;
  idRegra?: string;
  ehRecorrente: boolean;
  ativo: boolean;
  dataCadastro: string;
}

export interface ReceitaRequest {
  descricao: string;
  valor: number;
  data: string;
  idConta: string;
  idCategoria: string;
  idSubcategoria?: string;
  repete: boolean;
  dia?: number;
  diaUtil?: boolean;
  dataFim?: string;
}
