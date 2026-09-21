import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Contrato, ContratoRequest } from '../models/contrato.model';
import { Receita } from '../models/receita.model';

@Injectable({ providedIn: 'root' })
export class ContratoRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(ativo?: boolean): Observable<Contrato[]> {
    return this.get<Contrato[]>('/contratos', { ...(ativo !== undefined ? { ativo } : {}) });
  }

  obter(id: string): Observable<Contrato> {
    return this.get<Contrato>(`/contratos/${id}`);
  }

  criar(data: ContratoRequest): Observable<Contrato> {
    return this.post<Contrato>('/contratos', data);
  }

  atualizar(id: string, data: ContratoRequest): Observable<Contrato> {
    return this.put<Contrato>(`/contratos/${id}`, data);
  }

  encerrar(id: string): Observable<any> {
    return this.put<any>(`/contratos/${id}/encerrar`);
  }

  reativar(id: string): Observable<any> {
    return this.put<any>(`/contratos/${id}/reativar`);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/contratos/${id}`);
  }

  listarReceitas(id: string): Observable<Receita[]> {
    return this.get<Receita[]>(`/contratos/${id}/receitas`);
  }
}
