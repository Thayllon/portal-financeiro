import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Recorrente, RecorrenteRequest } from '../models/recorrente.model';

@Injectable({ providedIn: 'root' })
export class ReceitaRecorrenteRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(idUsuario: string): Observable<Recorrente[]> {
    return this.get<Recorrente[]>('/receitas-recorrentes', { idUsuario });
  }

  obter(id: string): Observable<Recorrente> {
    return this.get<Recorrente>(`/receitas-recorrentes/${id}`);
  }

  criar(idUsuario: string, data: RecorrenteRequest): Observable<Recorrente> {
    return this.post<Recorrente>('/receitas-recorrentes', data, { idUsuario });
  }

  atualizar(id: string, data: RecorrenteRequest): Observable<Recorrente> {
    return this.put<Recorrente>(`/receitas-recorrentes/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/receitas-recorrentes/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class DespesaRecorrenteRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(idUsuario: string): Observable<Recorrente[]> {
    return this.get<Recorrente[]>('/despesas-recorrentes', { idUsuario });
  }

  obter(id: string): Observable<Recorrente> {
    return this.get<Recorrente>(`/despesas-recorrentes/${id}`);
  }

  criar(idUsuario: string, data: RecorrenteRequest): Observable<Recorrente> {
    return this.post<Recorrente>('/despesas-recorrentes', data, { idUsuario });
  }

  atualizar(id: string, data: RecorrenteRequest): Observable<Recorrente> {
    return this.put<Recorrente>(`/despesas-recorrentes/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/despesas-recorrentes/${id}`);
  }
}
