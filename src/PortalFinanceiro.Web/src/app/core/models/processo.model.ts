export interface ProcessoEtapaItem {
  id: string;
  nome: string;
  descricao?: string;
  obrigatorio: boolean;
  exigeAnexo: boolean;
  ordem: number;
  concluida: boolean;
  dataInicio?: string;
  dataConclusao?: string;
  totalAnexos: number;
}

export interface ProcessoEtapa {
  id: string;
  nome: string;
  descricao?: string;
  ordem: number;
  concluida: boolean;
  dataPrevista?: string;
  dataInicio?: string;
  dataConclusao?: string;
  itens: ProcessoEtapaItem[];
}

export interface Processo {
  id: string;
  nome: string;
  descricao?: string;
  idParceria?: string;
  idContrato?: string;
  idModeloProcesso?: string;
  modeloNome: string;
  idCliente?: string;
  vinculoTipo: string;
  vinculoNome: string;
  cliente: string;
  ativo: boolean;
  dataEncerramento?: string;
  dataCadastro: string;
  faseAtual: string;
  totalEtapas: number;
  etapasConcluidas: number;
  totalItens: number;
  itensConcluidos: number;
  percentualConcluido: number;
  etapas: ProcessoEtapa[];
}

export interface ProcessoRequest {
  nome: string;
  descricao?: string;
  idParceria?: string;
  idContrato?: string;
  idModeloProcesso?: string;
  idCliente?: string;
}

export interface ProcessoEtapaRequest {
  nome: string;
  descricao?: string;
  dataPrevista?: string;
}

export interface ProcessoEtapaItemRequest {
  nome: string;
  descricao?: string;
  obrigatorio: boolean;
  exigeAnexo: boolean;
}
