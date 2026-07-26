import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="login-wrapper">
      <div class="login-card">
        <div class="login-header">
          <div class="logo">
            <svg width="32" height="32" viewBox="0 0 32 32" fill="none">
              <rect width="32" height="32" rx="8" fill="#0F766E"/>
              <path d="M8 16h16M16 8v16M10 10l12 12M22 10L10 22" stroke="#fff" stroke-width="2" stroke-linecap="round"/>
            </svg>
          </div>
          <h1>Portal Financeiro</h1>
          <p>Acesse sua conta para gerenciar suas finanças</p>
        </div>
        <form (ngSubmit)="onSubmit()" class="login-form">
          <div class="field">
            <label for="email">Email</label>
            <input
              id="email"
              type="email"
              [(ngModel)]="email"
              name="email"
              placeholder="seu&#64;email.com"
              required
              autocomplete="email"
            />
          </div>
          <div class="field">
            <label for="senha">Senha</label>
            <input
              id="senha"
              type="password"
              [(ngModel)]="senha"
              name="senha"
              placeholder="Sua senha"
              required
            />
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
    :host {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
    }

    .login-wrapper {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
      padding: 1rem;
    }

    .login-card {
      background: #fff;
      border-radius: 12px;
      padding: 2.5rem;
      width: 100%;
      max-width: 380px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
    }

    .login-header {
      text-align: center;
      margin-bottom: 2rem;
    }

    .logo {
      display: flex;
      justify-content: center;
      margin-bottom: 1rem;
    }

    .login-header h1 {
      font-size: 1.5rem;
      font-weight: 600;
      color: #0f172a;
      margin: 0 0 0.5rem 0;
    }

    .login-header p {
      font-size: 0.875rem;
      color: #64748b;
      margin: 0;
    }

    .login-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .field {
      display: flex;
      flex-direction: column;
      gap: 0.375rem;
    }

    .field label {
      font-size: 0.8125rem;
      font-weight: 500;
      color: #334155;
    }

    .field input {
      padding: 0.625rem 0.75rem;
      border: 1px solid #e2e8f0;
      border-radius: 6px;
      font-size: 0.875rem;
      color: #0f172a;
      background: #f8fafc;
      outline: none;
      transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }

    .field input::placeholder {
      color: #94a3b8;
    }

    .field input:focus {
      border-color: #0f766e;
      box-shadow: 0 0 0 3px rgba(15, 118, 110, 0.1);
      background: #fff;
    }

    button {
      width: 100%;
      padding: 0.75rem;
      background: #0f766e;
      color: #fff;
      border: none;
      border-radius: 6px;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: background 0.15s ease;
      margin-top: 0.5rem;
    }

    button:hover:not(:disabled) {
      background: #0d6b64;
    }

    button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .error {
      color: #dc2626;
      font-size: 0.8125rem;
      text-align: center;
      padding: 0.5rem;
      background: #fef2f2;
      border-radius: 6px;
    }
  `]
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = 'admin@portal.com';
  senha = '123456';
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
