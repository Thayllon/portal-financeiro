export interface Pessoa {
  id: string;
  nome: string;
  telefone: string | null;
  tipo: string;
  ativo: boolean;
  dataCadastro: string;
}

export interface PessoaRequest {
  nome: string;
  telefone: string;
  tipo: string;
}