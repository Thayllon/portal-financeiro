import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Regra, RegraRequest } from '../models/regra.model';

@Injectable({ providedIn: 'root' })
export class RegraReceitaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(): Observable<Regra[]> {
    return this.get<Regra[]>('/regras-receitas');
  }

  obter(id: string): Observable<Regra> {
    return this.get<Regra>(`/regras-receitas/${id}`);
  }

  atualizar(id: string, data: RegraRequest): Observable<Regra> {
    return this.put<Regra>(`/regras-receitas/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/regras-receitas/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class RegraDespesaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(): Observable<Regra[]> {
    return this.get<Regra[]>('/regras-despesas');
  }

  obter(id: string): Observable<Regra> {
    return this.get<Regra>(`/regras-despesas/${id}`);
  }

  atualizar(id: string, data: RegraRequest): Observable<Regra> {
    return this.put<Regra>(`/regras-despesas/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/regras-despesas/${id}`);
  }
}
