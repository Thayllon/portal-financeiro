import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Parceria, ParceriaRequest } from '../models/parceria.model';
import { Receita } from '../models/receita.model';
import { Despesa } from '../models/despesa.model';

@Injectable({ providedIn: 'root' })
export class ParceriaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(): Observable<Parceria[]> {
    return this.get<Parceria[]>('/parcerias');
  }

  obter(id: string): Observable<Parceria> {
    return this.get<Parceria>(`/parcerias/${id}`);
  }

  criar(data: ParceriaRequest): Observable<Parceria> {
    return this.post<Parceria>('/parcerias', data);
  }

  atualizar(id: string, data: ParceriaRequest): Observable<Parceria> {
    return this.put<Parceria>(`/parcerias/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/parcerias/${id}`);
  }

  listarReceitas(id: string): Observable<Receita[]> {
    return this.get<Receita[]>(`/parcerias/${id}/receitas`);
  }

  listarDespesas(id: string): Observable<Despesa[]> {
    return this.get<Despesa[]>(`/parcerias/${id}/despesas`);
  }
}
