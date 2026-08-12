export interface LoginResponse {
  token: string;
  usuarioId: string;
  nome: string;
  email: string;
  isAdmin: boolean;
  dataExpiracao: string;
}
