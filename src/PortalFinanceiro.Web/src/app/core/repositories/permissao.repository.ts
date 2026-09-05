import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Permissao } from '../models/permissao.model';

@Injectable({ providedIn: 'root' })
export class PermissaoRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(usuarioId: string): Observable<Permissao[]> {
    return this.get<Permissao[]>(`/usuarios/${usuarioId}/permissoes`);
  }

  salvar(usuarioId: string, permissoes: Permissao[]): Observable<any> {
    return this.put<any>(`/usuarios/${usuarioId}/permissoes`, permissoes);
  }
}
