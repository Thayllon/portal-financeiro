export interface Usuario {
  id: string;
  nome: string;
  email: string;
  isAdmin: boolean;
  ativo: boolean;
  dataCadastro: string;
}

export interface UsuarioRequest {
  nome: string;
  email: string;
  senha: string;
  isAdmin: boolean;
  ativo: boolean;
}
