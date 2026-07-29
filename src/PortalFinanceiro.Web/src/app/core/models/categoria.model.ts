export interface Categoria {
  id: string;
  nome: string;
  ativo: boolean;
  dataCadastro: string;
}

export interface CategoriaRequest {
  nome: string;
}
