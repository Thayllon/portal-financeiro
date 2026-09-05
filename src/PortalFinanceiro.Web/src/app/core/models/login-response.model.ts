export interface LoginResponse {
  token: string;
  usuarioId: string;
  nome: string;
  email: string;
  isAdmin: boolean;
  precisaTrocarSenha: boolean;
  dataExpiracao: string;
}
