import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="layout">
      <aside class="sidebar">
        <div class="sidebar-brand">
          <svg width="28" height="28" viewBox="0 0 32 32" fill="none">
            <rect width="32" height="32" rx="8" fill="#0f766e"/>
            <path d="M8 16h16M16 8v16M10 10l12 12M22 10L10 22" stroke="#fff" stroke-width="2" stroke-linecap="round"/>
          </svg>
          <span>Portal Financeiro</span>
        </div>
        <nav class="sidebar-nav">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/></svg>
            Dashboard
          </a>
          <a routerLink="/receitas" routerLinkActive="active">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
            Receitas
          </a>
          <a routerLink="/despesas" routerLinkActive="active">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
            Despesas
          </a>
          <a routerLink="/contas" routerLinkActive="active">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="5" width="20" height="14" rx="2"/><line x1="2" y1="10" x2="22" y2="10"/></svg>
            Contas
          </a>
          <a routerLink="/categorias/receita" routerLinkActive="active">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"/><line x1="7" y1="7" x2="7.01" y2="7"/></svg>
            Categorias
          </a>
        </nav>
        <div class="sidebar-footer">
          <div class="user-info">
            <div class="avatar">{{ initials() }}</div>
            <div class="user-details">
              <span class="user-name">{{ authService.user()?.nome }}</span>
              <span class="user-email">{{ authService.user()?.email }}</span>
            </div>
          </div>
          <button (click)="authService.logout()" class="logout-btn">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
          </button>
        </div>
      </aside>
      <main class="content">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    :host {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
    }

    .layout {
      display: flex;
      height: 100vh;
      overflow: hidden;
    }

    .sidebar {
      width: 240px;
      background: #0f172a;
      color: #e2e8f0;
      display: flex;
      flex-direction: column;
      flex-shrink: 0;
    }

    .sidebar-brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 1.25rem 1rem;
      border-bottom: 1px solid #1e293b;
    }

    .sidebar-brand span {
      font-size: 0.9375rem;
      font-weight: 600;
      color: #f8fafc;
    }

    .sidebar-nav {
      display: flex;
      flex-direction: column;
      gap: 0.125rem;
      padding: 0.75rem 0.5rem;
      flex: 1;
    }

    .sidebar-nav a {
      display: flex;
      align-items: center;
      gap: 0.625rem;
      padding: 0.5rem 0.75rem;
      border-radius: 6px;
      color: #94a3b8;
      font-size: 0.875rem;
      font-weight: 400;
      transition: all 0.15s ease;
    }

    .sidebar-nav a:hover {
      background: #1e293b;
      color: #e2e8f0;
    }

    .sidebar-nav a.active {
      background: #0f766e;
      color: #fff;
    }

    .sidebar-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 1rem;
      border-top: 1px solid #1e293b;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 0.625rem;
      min-width: 0;
    }

    .avatar {
      width: 32px;
      height: 32px;
      border-radius: 6px;
      background: #0f766e;
      color: #fff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 600;
      flex-shrink: 0;
    }

    .user-details {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .user-name {
      font-size: 0.8125rem;
      font-weight: 500;
      color: #e2e8f0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .user-email {
      font-size: 0.6875rem;
      color: #64748b;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .logout-btn {
      background: none;
      border: none;
      color: #64748b;
      padding: 0.375rem;
      border-radius: 4px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.15s ease;
    }

    .logout-btn:hover {
      background: #1e293b;
      color: #ef4444;
    }

    .content {
      flex: 1;
      overflow-y: auto;
      padding: 1.5rem;
      background: #f1f5f9;
    }
  `]
})
export class LayoutComponent {
  authService = inject(AuthService);

  initials(): string {
    const nome = this.authService.user()?.nome ?? '';
    return nome.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
