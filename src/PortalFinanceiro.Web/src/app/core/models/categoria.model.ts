export interface Categoria {
  id: string;
  nome: string;
  categoriaPaiId?: string;
  ativo: boolean;
  dataCadastro: string;
}

export interface CategoriaRequest {
  nome: string;
  categoriaPaiId?: string;
}
