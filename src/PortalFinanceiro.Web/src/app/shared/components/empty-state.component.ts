import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="empty">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" class="empty-icon">
        <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"/>
        <line x1="7" y1="7" x2="7.01" y2="7"/>
      </svg>
      <h3>{{ title() }}</h3>
      @if (description()) {
        <p>{{ description() }}</p>
      }
      @if (showAction()) {
        <button class="action-btn" (click)="action.emit()">{{ actionLabel() }}</button>
      }
    </div>
  `,
  styles: [`
    .empty { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 3rem 1rem; text-align: center; }
    .empty-icon { color: var(--text-muted); margin-bottom: 1rem; }
    h3 { font-size: 1rem; font-weight: 600; color: var(--text-primary); margin-bottom: 0.5rem; }
    p { font-size: 0.875rem; color: var(--text-muted); max-width: 320px; line-height: 1.5; margin-bottom: 1.25rem; }
    .action-btn { padding: 0.5rem 1rem; background: var(--color-primary); color: #fff; border: none; border-radius: var(--radius-md); font-size: 0.875rem; font-weight: 500; transition: background var(--transition-fast); }
    .action-btn:hover { background: var(--color-primary-hover); }
  `]
})
export class EmptyStateComponent {
  title = input('');
  description = input('');
  showAction = input(true);
  actionLabel = input('Começar');
  action = output<void>();
}
