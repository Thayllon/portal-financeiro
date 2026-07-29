export interface ContaBancaria {
  id: string;
  nome: string;
  banco: string;
  tipo: string;
  ativo: boolean;
  dataCadastro: string;
}

export interface ContaBancariaRequest {
  nome: string;
  banco: string;
  tipo: string;
}
