export interface ReceitaServico {
  id: string;
  categoriaServicoId: string;
  categoriaServico: string;
  subcategoriaServicoId?: string;
  subcategoriaServico: string;
}

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
  idParceiro?: string;
  parceiro: string;
  idCliente?: string;
  cliente: string;
  idParceria?: string;
  parceria: string;
  parceriaValor?: number;
  servicos: ReceitaServico[];
  status: number;
  dataRealizacao?: string;
  idRegra?: string;
  ehRecorrente: boolean;
  ativo: boolean;
  dataCadastro: string;
}

export interface ReceitaServicoRequest {
  categoriaServicoId: string;
  subcategoriaServicoId?: string;
}

export interface ReceitaRequest {
  descricao: string;
  valor: number;
  data: string;
  idConta: string;
  idCategoria: string;
  idSubcategoria?: string;
  idParceiro?: string;
  idCliente?: string;
  idParceria?: string;
  servicos?: ReceitaServicoRequest[];
  repete: boolean;
  dia?: number;
  diaUtil?: boolean;
  dataFim?: string;
}
