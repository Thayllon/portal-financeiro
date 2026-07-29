import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { LancamentoMensal, StatusRequest } from '../models/lancamento-mensal.model';

@Injectable({ providedIn: 'root' })
export class ReceitaMensalRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listarPorMes(idUsuario: string, mes: number, ano: number): Observable<LancamentoMensal[]> {
    return this.get<LancamentoMensal[]>('/receitas-mensais', { idUsuario, mes, ano });
  }

  receber(id: string, data: StatusRequest): Observable<LancamentoMensal> {
    return this.post<LancamentoMensal>(`/receitas-mensais/${id}/receber`, data);
  }

  estornar(id: string): Observable<LancamentoMensal> {
    return this.post<LancamentoMensal>(`/receitas-mensais/${id}/estornar`);
  }
}

@Injectable({ providedIn: 'root' })
export class DespesaMensalRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listarPorMes(idUsuario: string, mes: number, ano: number): Observable<LancamentoMensal[]> {
    return this.get<LancamentoMensal[]>('/despesas-mensais', { idUsuario, mes, ano });
  }

  pagar(id: string, data: StatusRequest): Observable<LancamentoMensal> {
    return this.post<LancamentoMensal>(`/despesas-mensais/${id}/pagar`, data);
  }

  estornar(id: string): Observable<LancamentoMensal> {
    return this.post<LancamentoMensal>(`/despesas-mensais/${id}/estornar`);
  }
}
