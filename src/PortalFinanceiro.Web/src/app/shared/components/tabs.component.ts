import { Component, input, output } from '@angular/core';

export interface Tab {
  id: string;
  label: string;
}

@Component({
  selector: 'app-tabs',
  standalone: true,
  template: `
    <div class="tabs">
      @for (tab of tabs(); track tab.id) {
        <button class="tab" [class.tab--active]="active() === tab.id" (click)="change.emit(tab.id)">
          {{ tab.label }}
        </button>
      }
    </div>
  `,
  styles: [`
    .tabs { display: flex; gap: 0; border-bottom: 1px solid var(--surface-border); margin-bottom: 1.5rem; }
    .tab {
      padding: 0.625rem 1rem; background: none; border: none; border-bottom: 2px solid transparent;
      font-size: 0.875rem; font-weight: 500; color: var(--text-muted);
      transition: all var(--transition-fast); margin-bottom: -1px;
      position: relative;
    }
    .tab:hover { color: var(--text-secondary); }
    .tab--active {
      color: var(--color-primary);
      border-bottom-color: var(--color-primary);
    }
  `]
})
export class TabsComponent {
  tabs = input<Tab[]>([]);
  active = input<string>('');
  change = output<string>();
}
