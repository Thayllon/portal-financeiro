import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-month-nav',
  standalone: true,
  template: `
    <div class="month-nav">
      <button class="nav-btn" (click)="prev.emit()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="m15 18-6-6 6-6"/></svg>
      </button>
      <span class="month-label">{{ mes() }}/{{ ano() }}</span>
      <button class="nav-btn" (click)="next.emit()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="m9 18 6-6-6-6"/></svg>
      </button>
    </div>
  `,
  styles: [`
    .month-nav { display: flex; align-items: center; gap: 0.75rem; }
    .nav-btn {
      background: var(--content-surface); border: 1px solid var(--surface-border);
      border-radius: var(--radius-md); padding: 0.375rem; display: flex;
      color: var(--text-secondary); transition: all var(--transition-fast);
    }
    .nav-btn:hover { border-color: var(--color-primary); color: var(--color-primary); }
    .month-label { font-size: 1rem; font-weight: 600; color: var(--text-primary); min-width: 7rem; text-align: center; }
  `]
})
export class MonthNavComponent {
  mes = input<number>(0);
  ano = input<number>(0);
  prev = output<void>();
  next = output<void>();
}
