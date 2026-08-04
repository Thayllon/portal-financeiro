import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Despesa, DespesaRequest } from '../models/despesa.model';
import { StatusRequest } from '../models/status.model';

export interface DespesaFiltros {
  mes: number;
  ano: number;
  idConta?: string;
  status?: number;
  idCategoria?: string;
  busca?: string;
}

@Injectable({ providedIn: 'root' })
export class DespesaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(filtros: DespesaFiltros): Observable<Despesa[]> {
    return this.get<Despesa[]>('/despesas', filtros);
  }

  obter(id: string): Observable<Despesa> {
    return this.get<Despesa>(`/despesas/${id}`);
  }

  criar(data: DespesaRequest): Observable<Despesa> {
    return this.post<Despesa>('/despesas', data);
  }

  atualizar(id: string, data: DespesaRequest): Observable<Despesa> {
    return this.put<Despesa>(`/despesas/${id}`, data);
  }

  pagar(id: string, data: StatusRequest): Observable<Despesa> {
    return this.post<Despesa>(`/despesas/${id}/pagar`, data);
  }

  estornar(id: string): Observable<Despesa> {
    return this.post<Despesa>(`/despesas/${id}/estornar`);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/despesas/${id}`);
  }
}
