import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Receita, ReceitaRequest } from '../models/receita.model';
import { StatusRequest } from '../models/status.model';

export interface ReceitaFiltros {
  mes: number;
  ano: number;
  idConta?: string;
  status?: number;
  idCategoria?: string;
  busca?: string;
}

@Injectable({ providedIn: 'root' })
export class ReceitaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(filtros: ReceitaFiltros): Observable<Receita[]> {
    return this.get<Receita[]>('/receitas', filtros);
  }

  obter(id: string): Observable<Receita> {
    return this.get<Receita>(`/receitas/${id}`);
  }

  criar(data: ReceitaRequest): Observable<Receita> {
    return this.post<Receita>('/receitas', data);
  }

  atualizar(id: string, data: ReceitaRequest): Observable<Receita> {
    return this.put<Receita>(`/receitas/${id}`, data);
  }

  receber(id: string, data: StatusRequest): Observable<Receita> {
    return this.post<Receita>(`/receitas/${id}/receber`, data);
  }

  estornar(id: string): Observable<Receita> {
    return this.post<Receita>(`/receitas/${id}/estornar`);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/receitas/${id}`);
  }
}
