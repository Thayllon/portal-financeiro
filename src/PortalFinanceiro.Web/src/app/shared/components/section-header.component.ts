import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-section-header',
  standalone: true,
  template: `
    <div class="section-header">
      <div>
        <h1>{{ title() }}</h1>
        @if (subtitle()) {
          <p class="subtitle">{{ subtitle() }}</p>
        }
      </div>
      @if (showAdd()) {
        <button class="add-btn" (click)="add.emit()">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M12 5v14M5 12h14"/></svg>
          {{ addLabel() }}
        </button>
      }
    </div>
  `,
  styles: [`
    .section-header { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 1.5rem; gap: 1rem; }
    h1 { font-size: 1.5rem; font-weight: 600; color: var(--text-primary); }
    .subtitle { font-size: 0.875rem; color: var(--text-muted); margin-top: 0.25rem; }
    .add-btn {
      display: flex; align-items: center; gap: 0.375rem; padding: 0.5rem 1rem;
      background: var(--color-primary); color: #fff; border: none; border-radius: var(--radius-md);
      font-size: 0.875rem; font-weight: 500; white-space: nowrap;
      transition: background var(--transition-fast);
    }
    .add-btn:hover { background: var(--color-primary-hover); }
  `]
})
export class SectionHeaderComponent {
  title = input('');
  subtitle = input('');
  showAdd = input(true);
  addLabel = input('Novo');
  add = output<void>();
}
