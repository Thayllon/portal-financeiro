export interface Permissao {
  modulo: string;
  nivel: number;
}

export const MODULOS = [
  { id: 'dashboard', nome: 'Dashboard' },
  { id: 'receitas', nome: 'Receitas' },
  { id: 'despesas', nome: 'Despesas' },
  { id: 'contas', nome: 'Contas bancárias' },
  { id: 'categorias', nome: 'Categorias' },
  { id: 'clientes', nome: 'Clientes' },
  { id: 'parceiros', nome: 'Parceiros' },
] as const;

export const NivelPermissao = {
  Nenhum: 0,
  Leitura: 1,
  Escrita: 2,
} as const;
