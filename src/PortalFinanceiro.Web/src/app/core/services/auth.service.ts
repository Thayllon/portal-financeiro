import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { AuthRepository } from '../repositories/auth.repository';
import { User } from '../models/user.model';
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
        const user: User = {
          usuarioId: response.usuarioId,
          nome: response.nome,
          email: response.email,
          isAdmin: response.isAdmin,
          token: response.token
        };
        this.saveUser(user);
        this.userSignal.set(user);
      })
    );
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
      return JSON.parse(stored);
    } catch {
      return null;
    }
  }
}
