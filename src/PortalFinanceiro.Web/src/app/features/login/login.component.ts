import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <h1>Portal Financeiro</h1>
        <form (ngSubmit)="onSubmit()">
          <div class="field">
            <label for="email">Email</label>
            <input id="email" type="email" [(ngModel)]="email" name="email" placeholder="seu@email.com" required autocomplete="email" />
          </div>
          <div class="field">
            <label for="senha">Senha</label>
            <input id="senha" type="password" [(ngModel)]="senha" name="senha" placeholder="Sua senha" required />
          </div>
          @if (erro) {
            <div class="error">{{ erro }}</div>
          }
          <button type="submit" [disabled]="loading">
            {{ loading ? 'Entrando...' : 'Entrar' }}
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-container { display: flex; align-items: center; justify-content: center; height: 100vh; background: #1a1a2e; }
    .login-card { background: #fff; padding: 2rem; border-radius: 8px; width: 100%; max-width: 400px; }
    .login-card h1 { text-align: center; margin-bottom: 1.5rem; color: #1a1a2e; }
    .field { margin-bottom: 1rem; }
    .field label { display: block; margin-bottom: 0.25rem; color: #333; }
    .field input { width: 100%; padding: 0.5rem; border: 1px solid #ccc; border-radius: 4px; }
    button { width: 100%; padding: 0.75rem; background: #0f766e; color: #fff; border: none; border-radius: 4px; cursor: pointer; }
    button:disabled { opacity: 0.5; }
    .error { color: #dc2626; margin-bottom: 1rem; font-size: 0.875rem; }
  `]
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  senha = '';
  loading = false;
  erro = '';

  onSubmit() {
    this.loading = true;
    this.erro = '';
    this.authService.login(this.email, this.senha).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => {
        this.erro = 'Email ou senha inválidos.';
        this.loading = false;
      }
    });
  }
}
