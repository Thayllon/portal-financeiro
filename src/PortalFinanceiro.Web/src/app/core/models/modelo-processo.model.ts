export interface ModeloItem {
  id: string;
  nome: string;
  descricao?: string;
  obrigatorio: boolean;
  exigeAnexo: boolean;
  ordem: number;
}

export interface ModeloEtapa {
  id: string;
  nome: string;
  descricao?: string;
  ordem: number;
  itens: ModeloItem[];
}

export interface ModeloProcesso {
  id: string;
  nome: string;
  descricao?: string;
  ativo: boolean;
  dataCadastro: string;
  totalEtapas: number;
  totalItens: number;
  etapas: ModeloEtapa[];
}

export interface ModeloProcessoRequest {
  nome: string;
  descricao?: string;
}

export interface ModeloEtapaRequest {
  nome: string;
  descricao?: string;
}

export interface ModeloItemRequest {
  nome: string;
  descricao?: string;
  obrigatorio: boolean;
  exigeAnexo: boolean;
}
