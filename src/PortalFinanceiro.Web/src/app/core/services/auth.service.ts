import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { AuthRepository } from '../repositories/auth.repository';
import { User } from '../models/user.model';
import { LoginResponse } from '../models/login-response.model';
import { Permissao, NivelPermissao } from '../models/permissao.model';
import { tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly STORAGE_KEY = 'portal-financeiro.auth';
  private userSignal = signal<User | null>(this.loadUser());

  user = this.userSignal.asReadonly();
  isAuthenticated = computed(() => this.userSignal() !== null);
  isAdmin = computed(() => this.userSignal()?.isAdmin === true);

  constructor(
    private authRepository: AuthRepository,
    private router: Router
  ) {}

  login(email: string, senha: string) {
    return this.authRepository.login(email, senha).pipe(
      tap(response => {
        if (!response.precisaTrocarSenha) {
          this.setSession(response);
        }
      })
    );
  }

  trocarSenha(email: string, senhaAtual: string, novaSenha: string) {
    return this.authRepository.alterarSenha(email, senhaAtual, novaSenha).pipe(
      tap(response => this.setSession(response))
    );
  }

  temPermissao(modulo: string, nivelMinimo: number = NivelPermissao.Leitura): boolean {
    const u = this.userSignal();
    if (!u) return false;
    if (u.isAdmin) return true;
    const p = u.permissoes.find(x => x.modulo === modulo);
    return !!p && p.nivel >= nivelMinimo;
  }

  private setSession(response: LoginResponse) {
    const user: User = {
      usuarioId: response.usuarioId,
      nome: response.nome,
      email: response.email,
      isAdmin: response.isAdmin,
      token: response.token,
      permissoes: response.permissoes ?? []
    };
    this.saveUser(user);
    this.userSignal.set(user);
  }

  logout() {
    localStorage.removeItem(this.STORAGE_KEY);
    this.userSignal.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.userSignal()?.token ?? null;
  }

  private saveUser(user: User) {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(user));
  }

  private loadUser(): User | null {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    if (!stored) return null;

    try {
      const parsed = JSON.parse(stored);
      return { ...parsed, permissoes: parsed.permissoes ?? [] };
    } catch {
      return null;
    }
  }
}
