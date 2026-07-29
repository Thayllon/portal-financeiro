import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { ContaBancaria, ContaBancariaRequest } from '../models/conta-bancaria.model';

@Injectable({ providedIn: 'root' })
export class ContaBancariaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(idUsuario: string): Observable<ContaBancaria[]> {
    return this.get<ContaBancaria[]>('/contas-bancarias', { idUsuario });
  }

  obter(id: string): Observable<ContaBancaria> {
    return this.get<ContaBancaria>(`/contas-bancarias/${id}`);
  }

  criar(idUsuario: string, data: ContaBancariaRequest): Observable<ContaBancaria> {
    return this.post<ContaBancaria>('/contas-bancarias', data, { idUsuario });
  }

  atualizar(id: string, data: ContaBancariaRequest): Observable<ContaBancaria> {
    return this.put<ContaBancaria>(`/contas-bancarias/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/contas-bancarias/${id}`);
  }
}
