import { TipoPessoa } from './enums';

export interface Pessoa {
  id: string;
  nome: string;
  telefone: string | null;
  tipo: TipoPessoa;
  ativo: boolean;
  dataCadastro: string;
}

export interface PessoaRequest {
  nome: string;
  telefone: string;
  tipo: TipoPessoa;
}