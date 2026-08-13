import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Regra, RegraRequest } from '../models/regra.model';

class RegraBaseRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  constructor(private rota: string) { super(); }

  listar(): Observable<Regra[]> {
    return this.get<Regra[]>(`/regras-${this.rota}`);
  }

  obter(id: string): Observable<Regra> {
    return this.get<Regra>(`/regras-${this.rota}/${id}`);
  }

  atualizar(id: string, data: RegraRequest): Observable<Regra> {
    return this.put<Regra>(`/regras-${this.rota}/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/regras-${this.rota}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class RegraReceitaRepository extends RegraBaseRepository {
  constructor() { super('receitas'); }
}

@Injectable({ providedIn: 'root' })
export class RegraDespesaRepository extends RegraBaseRepository {
  constructor() { super('despesas'); }
}
