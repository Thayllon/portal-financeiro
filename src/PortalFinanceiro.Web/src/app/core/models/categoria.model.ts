export interface Categoria {
  id: string;
  idUsuario: string;
  nome: string;
  categoriaPaiId?: string;
  ativo: boolean;
  podeEditar: boolean;
  dataCadastro: string;
}

export interface CategoriaRequest {
  nome: string;
  categoriaPaiId?: string;
}
