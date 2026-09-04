import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Categoria, CategoriaRequest } from '../models/categoria.model';

class CategoriaBaseRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  constructor(private rota: string) { super(); }

  listar(): Observable<Categoria[]> {
    return this.get<Categoria[]>(`/categorias/${this.rota}`);
  }

  obter(id: string): Observable<Categoria> {
    return this.get<Categoria>(`/categorias/${this.rota}/${id}`);
  }

  criar(data: CategoriaRequest): Observable<Categoria> {
    return this.post<Categoria>(`/categorias/${this.rota}`, data);
  }

  atualizar(id: string, data: CategoriaRequest): Observable<Categoria> {
    return this.put<Categoria>(`/categorias/${this.rota}/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/categorias/${this.rota}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class CategoriaReceitaRepository extends CategoriaBaseRepository {
  constructor() { super('receita'); }
}

@Injectable({ providedIn: 'root' })
export class CategoriaDespesaRepository extends CategoriaBaseRepository {
  constructor() { super('despesa'); }
}

@Injectable({ providedIn: 'root' })
export class CategoriaServicoRepository extends CategoriaBaseRepository {
  constructor() { super('servicos'); }
}
