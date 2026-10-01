import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Receita, ReceitaRequest } from '../models/receita.model';
import { Despesa, DespesaRequest } from '../models/despesa.model';
import { StatusRequest } from '../models/status.model';

export interface LancamentoFiltros {
  mes: number;
  ano: number;
  idConta?: string;
  status?: number;
  idCategoria?: string;
  busca?: string;
}

export type ReceitaFiltros = LancamentoFiltros;
export type DespesaFiltros = LancamentoFiltros;

@Injectable({ providedIn: 'root' })
export class ReceitaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(filtros: LancamentoFiltros): Observable<Receita[]> {
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

@Injectable({ providedIn: 'root' })
export class DespesaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(filtros: LancamentoFiltros): Observable<Despesa[]> {
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
