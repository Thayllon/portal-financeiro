export type ModuloPermissao =
  | 'home'
  | 'dashboard'
  | 'receitas'
  | 'despesas'
  | 'parcerias'
  | 'contratos'
  | 'processos'
  | 'contas'
  | 'categorias'
  | 'clientes'
  | 'parceiros'
  | 'usuarios'
  | 'fluxo-adicional-receita'
  | 'fluxo-adicional-despesa'
  | 'outros-indicadores'
  | 'qa';

export interface Permissao {
  modulo: ModuloPermissao;
  nivel: number;
}

export const NivelPermissao = {
  Nenhum: 0,
  Leitura: 1,
  Escrita: 2,
} as const;

export const MODULO_FLUXO_ADICIONAL: ModuloPermissao = 'fluxo-adicional-receita';
export const MODULO_FLUXO_ADICIONAL_DESPESA: ModuloPermissao = 'fluxo-adicional-despesa';
export const MODULO_OUTROS_INDICADORES: ModuloPermissao = 'outros-indicadores';
export const MODULO_QA: ModuloPermissao = 'qa';
