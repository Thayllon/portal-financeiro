import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { ProLabore, ProLaboreRequest } from '../models/pro-labore.model';

@Injectable({ providedIn: 'root' })
export class ProLaboreRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(): Observable<ProLabore[]> {
    return this.get<ProLabore[]>('/pro-labores');
  }

  criar(data: ProLaboreRequest): Observable<ProLabore> {
    return this.post<ProLabore>('/pro-labores', data);
  }

  atualizar(id: string, data: ProLaboreRequest): Observable<ProLabore> {
    return this.put<ProLabore>(`/pro-labores/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/pro-labores/${id}`);
  }
}
