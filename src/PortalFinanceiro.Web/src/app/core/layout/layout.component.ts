import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastComponent } from '../../shared/components/toast.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastComponent, ConfirmDialogComponent, LucideDynamicIcon],
  template: `
    <app-toast />
    <app-confirm-dialog />
    <div class="layout" [class.layout--collapsed]="sidebarCollapsed()">
      <aside class="sidebar">
        <div class="sidebar-brand">
          <svg width="28" height="28" viewBox="0 0 32 32" fill="none">
            <rect width="32" height="32" rx="8" fill="#0f766e"/>
            <path d="M8 16h16M16 8v16M10 10l12 12M22 10L10 22" stroke="#fff" stroke-width="2" stroke-linecap="round"/>
          </svg>
          @if (!sidebarCollapsed()) {
            <span>Portal Financeiro</span>
          }
        </div>
        <nav class="sidebar-nav">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">
            <svg lucideIcon="layout-dashboard" [size]="18" />
            @if (!sidebarCollapsed()) {
              <span>Dashboard</span>
            }
          </a>
          <a routerLink="/receitas" routerLinkActive="active">
            <svg lucideIcon="trending-up" [size]="18" />
            @if (!sidebarCollapsed()) {
              <span>Receitas</span>
            }
          </a>
          <a routerLink="/despesas" routerLinkActive="active">
            <svg lucideIcon="trending-down" [size]="18" />
            @if (!sidebarCollapsed()) {
              <span>Despesas</span>
            }
          </a>
          <a routerLink="/contas" routerLinkActive="active">
            <svg lucideIcon="wallet" [size]="18" />
            @if (!sidebarCollapsed()) {
              <span>Contas</span>
            }
          </a>
          <a routerLink="/categorias" routerLinkActive="active">
            <svg lucideIcon="tag" [size]="18" />
            @if (!sidebarCollapsed()) {
              <span>Categorias</span>
            }
          </a>
        </nav>
        <button class="sidebar-toggle" (click)="toggleSidebar()">
          <svg [lucideIcon]="sidebarCollapsed() ? 'chevron-right' : 'chevron-left'" [size]="16" />
        </button>
        <div class="sidebar-footer">
          @if (!sidebarCollapsed()) {
            <div class="user-info">
              <div class="avatar">{{ initials() }}</div>
              <div class="user-details">
                <span class="user-name">{{ authService.user()?.nome }}</span>
                <span class="user-email">{{ authService.user()?.email }}</span>
              </div>
            </div>
          }
          <button (click)="authService.logout()" class="logout-btn" [title]="sidebarCollapsed() ? 'Sair' : ''">
            <svg lucideIcon="log-out" [size]="18" />
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
      height: calc(100vh - 24px);
      margin: 12px;
      overflow: hidden;
      border-radius: 16px;
      box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
    }

    .sidebar {
      width: 240px;
      background: #0f172a;
      color: #e2e8f0;
      display: flex;
      flex-direction: column;
      flex-shrink: 0;
      transition: width 0.2s ease;
      overflow: visible;
      position: relative;
    }

    .layout--collapsed .sidebar {
      width: 60px;
    }

    .sidebar-brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 1.25rem 1rem;
      border-bottom: 1px solid #1e293b;
      min-height: 60px;
      overflow: hidden;
    }

    .sidebar-brand span {
      font-size: 0.9375rem;
      font-weight: 600;
      color: #f8fafc;
      white-space: nowrap;
    }

    .sidebar-nav {
      display: flex;
      flex-direction: column;
      gap: 0.125rem;
      padding: 0.75rem 0.5rem;
      flex: 1;
      overflow: hidden;
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
      white-space: nowrap;
    }

    .sidebar-nav a:hover {
      background: #1e293b;
      color: #e2e8f0;
    }

    .sidebar-nav a.active {
      background: #0f766e;
      color: #fff;
    }

    .layout--collapsed .sidebar-nav a {
      justify-content: center;
      padding: 0.625rem;
    }

    .sidebar-toggle {
      position: absolute;
      top: 50%;
      right: -14px;
      transform: translateY(-50%);
      z-index: 11;
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: #0f766e;
      border: 2px solid #1e293b;
      color: #fff;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.2s ease;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
    }

    .sidebar-toggle:hover {
      background: #0d6b64;
      transform: translateY(-50%) scale(1.1);
    }

    .sidebar-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 1rem;
      border-top: 1px solid #1e293b;
      overflow: hidden;
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
      background: #f1f5f9;
      position: relative;
    }

    @media (max-width: 767px) {
      .layout {
        margin: 0;
        height: 100vh;
        border-radius: 0;
      }
      .sidebar {
        position: fixed;
        z-index: 100;
        height: 100vh;
      }
      .layout--collapsed .sidebar {
        width: 0;
        padding: 0;
      }
      .layout--collapsed .sidebar-toggle {
        right: 14px;
      }
      .content {
        margin-left: 0 !important;
      }
    }
  `]
})
export class LayoutComponent {
  authService = inject(AuthService);
  sidebarCollapsed = signal(false);

  toggleSidebar() {
    this.sidebarCollapsed.update(v => !v);
  }

  initials(): string {
    const nome = this.authService.user()?.nome ?? '';
    return nome.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
