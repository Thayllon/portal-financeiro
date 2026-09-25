export interface Permissao {
  modulo: string;
  nivel: number;
}

export const NivelPermissao = {
  Nenhum: 0,
  Leitura: 1,
  Escrita: 2,
} as const;

export const MODULO_FLUXO_ADICIONAL = 'fluxo-adicional-receita';
export const MODULO_FLUXO_ADICIONAL_DESPESA = 'fluxo-adicional-despesa';
export const MODULO_QA = 'qa';
