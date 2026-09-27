export interface ProcessoEtapa {
  id: string;
  nome: string;
  descricao?: string;
  ordem: number;
  concluida: boolean;
  dataPrevista?: string;
  dataConclusao?: string;
}

export interface Processo {
  id: string;
  nome: string;
  descricao?: string;
  idParceria?: string;
  idContrato?: string;
  vinculoTipo: string;
  vinculoNome: string;
  cliente: string;
  ativo: boolean;
  dataCadastro: string;
  totalEtapas: number;
  etapasConcluidas: number;
  percentualConcluido: number;
  etapas: ProcessoEtapa[];
}

export interface ProcessoRequest {
  nome: string;
  descricao?: string;
  idParceria?: string;
  idContrato?: string;
}

export interface ProcessoEtapaRequest {
  nome: string;
  descricao?: string;
  dataPrevista?: string;
}
