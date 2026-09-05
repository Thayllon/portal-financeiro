import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Usuario, UsuarioRequest } from '../models/usuario.model';

@Injectable({ providedIn: 'root' })
export class UsuarioRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(): Observable<Usuario[]> {
    return this.get<Usuario[]>('/usuarios');
  }

  criar(data: UsuarioRequest): Observable<Usuario> {
    return this.post<Usuario>('/usuarios', data);
  }

  atualizar(id: string, data: UsuarioRequest): Observable<Usuario> {
    return this.put<Usuario>(`/usuarios/${id}`, data);
  }

  alterarAtivo(id: string, ativo: boolean): Observable<any> {
    return this.patch<any>(`/usuarios/${id}/ativo`, { ativo });
  }

  resetarSenha(id: string): Observable<any> {
    return this.patch<any>(`/usuarios/${id}/senha`);
  }
}
