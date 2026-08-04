import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Categoria, CategoriaRequest } from '../models/categoria.model';

@Injectable({ providedIn: 'root' })
export class CategoriaReceitaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(idUsuario: string): Observable<Categoria[]> {
    return this.get<Categoria[]>('/categorias/receita', { idUsuario });
  }

  obter(id: string): Observable<Categoria> {
    return this.get<Categoria>(`/categorias/receita/${id}`);
  }

  criar(data: CategoriaRequest): Observable<Categoria> {
    return this.post<Categoria>('/categorias/receita', data);
  }

  atualizar(id: string, data: CategoriaRequest): Observable<Categoria> {
    return this.put<Categoria>(`/categorias/receita/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/categorias/receita/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class CategoriaDespesaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(idUsuario: string): Observable<Categoria[]> {
    return this.get<Categoria[]>('/categorias/despesa', { idUsuario });
  }

  obter(id: string): Observable<Categoria> {
    return this.get<Categoria>(`/categorias/despesa/${id}`);
  }

  criar(data: CategoriaRequest): Observable<Categoria> {
    return this.post<Categoria>('/categorias/despesa', data);
  }

  atualizar(id: string, data: CategoriaRequest): Observable<Categoria> {
    return this.put<Categoria>(`/categorias/despesa/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/categorias/despesa/${id}`);
  }
}
