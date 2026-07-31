import { Component, input, output } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-section-header',
  standalone: true,
  imports: [LucideDynamicIcon],
  template: `
    <div class="section-header">
      <div class="section-header__left">
        @if (icon()) {
          <svg lucideIcon [lucideIcon]="icon()!" class="section-header__icon" [size]="20" />
        }
        <div>
          <h1>{{ title() }}</h1>
          @if (subtitle()) {
            <p class="subtitle">{{ subtitle() }}</p>
          }
        </div>
      </div>
      @if (showAdd()) {
        <button class="add-btn" (click)="add.emit()">
          <svg lucideIcon="plus" [size]="16" />
          {{ addLabel() }}
        </button>
      }
    </div>
  `,
  styles: [`
    .section-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 1.5rem;
      gap: 1rem;
    }
    .section-header__left {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .section-header__icon {
      color: var(--color-primary);
      flex-shrink: 0;
    }
    h1 {
      font-size: 1.5rem;
      font-weight: 600;
      color: var(--text-primary);
    }
    .subtitle {
      font-size: 0.875rem;
      color: var(--text-muted);
      margin-top: 0.25rem;
    }
    .add-btn {
      display: flex;
      align-items: center;
      gap: 0.375rem;
      padding: 0.5rem 1rem;
      background: var(--color-primary);
      color: #fff;
      border: none;
      border-radius: var(--radius-md);
      font-size: 0.875rem;
      font-weight: 500;
      white-space: nowrap;
      transition: background var(--transition-fast);
    }
    .add-btn:hover { background: var(--color-primary-hover); }
    @media (max-width: 767px) {
      .section-header { flex-direction: column; align-items: stretch; }
      h1 { font-size: 1.25rem; }
    }
  `]
})
export class SectionHeaderComponent {
  title = input('');
  subtitle = input('');
  icon = input<string | null>(null);
  showAdd = input(true);
  addLabel = input('Novo');
  add = output<void>();
}
