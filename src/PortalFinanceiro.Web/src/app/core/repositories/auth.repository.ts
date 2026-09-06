import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { LoginResponse } from '../models/login-response.model';

@Injectable({ providedIn: 'root' })
export class AuthRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  login(email: string, senha: string): Observable<LoginResponse> {
    return this.post<LoginResponse>('/auth/login', { email, senha });
  }

  alterarSenha(email: string, senhaAtual: string, novaSenha: string): Observable<LoginResponse> {
    return this.post<LoginResponse>('/auth/alterar-senha', { email, senhaAtual, novaSenha });
  }
}
