import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Pessoa, PessoaRequest } from '../models/pessoa.model';

@Injectable({ providedIn: 'root' })
export class PessoaRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(): Observable<Pessoa[]> {
    return this.get<Pessoa[]>('/pessoas');
  }

  obter(id: string): Observable<Pessoa> {
    return this.get<Pessoa>(`/pessoas/${id}`);
  }

  criar(data: PessoaRequest): Observable<Pessoa> {
    return this.post<Pessoa>('/pessoas', data);
  }

  atualizar(id: string, data: PessoaRequest): Observable<Pessoa> {
    return this.put<Pessoa>(`/pessoas/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/pessoas/${id}`);
  }
}