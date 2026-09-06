import { Permissao } from './permissao.model';

export interface User {
  usuarioId: string;
  nome: string;
  email: string;
  isAdmin: boolean;
  token: string;
  permissoes: Permissao[];
}
