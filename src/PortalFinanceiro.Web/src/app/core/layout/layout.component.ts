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
        <div class="sidebar-header">
          <h2>Portal Financeiro</h2>
        </div>
        <nav class="sidebar-nav">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">Dashboard</a>
          <a routerLink="/receitas" routerLinkActive="active">Receitas</a>
          <a routerLink="/despesas" routerLinkActive="active">Despesas</a>
          <a routerLink="/contas" routerLinkActive="active">Contas</a>
          <a routerLink="/categorias/receita" routerLinkActive="active">Categorias Receita</a>
          <a routerLink="/categorias/despesa" routerLinkActive="active">Categorias Despesa</a>
        </nav>
        <div class="sidebar-footer">
          <span>{{ authService.user()?.nome }}</span>
          <button (click)="authService.logout()">Sair</button>
        </div>
      </aside>
      <main class="content">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    .layout { display: flex; height: 100vh; }
    .sidebar { width: 260px; background: #1a1a2e; color: #fff; display: flex; flex-direction: column; padding: 1rem; }
    .sidebar-header h2 { font-size: 1.2rem; margin-bottom: 2rem; }
    .sidebar-nav { display: flex; flex-direction: column; gap: 0.5rem; flex: 1; }
    .sidebar-nav a { color: #ccc; text-decoration: none; padding: 0.5rem; border-radius: 4px; }
    .sidebar-nav a:hover, .sidebar-nav a.active { background: #16213e; color: #fff; }
    .sidebar-footer { border-top: 1px solid #333; padding-top: 1rem; display: flex; flex-direction: column; gap: 0.5rem; }
    .sidebar-footer button { background: none; border: 1px solid #666; color: #fff; padding: 0.25rem 0.5rem; border-radius: 4px; cursor: pointer; }
    .content { flex: 1; padding: 2rem; background: #f5f5f5; overflow-y: auto; }
  `]
})
export class LayoutComponent {
  authService = inject(AuthService);
}
