import { Component, input } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-page',
  standalone: true,
  imports: [LucideDynamicIcon],
  template: `
    <div class="page">
      <header class="page__header">
        <div class="page__header-left">
          @if (icon()) {
            <svg lucideIcon [lucideIcon]="icon()!" class="page__icon" [size]="22" />
          }
          <div>
            <h1 class="page__title">{{ title() }}</h1>
            @if (subtitle()) {
              <p class="page__subtitle">{{ subtitle() }}</p>
            }
          </div>
        </div>
        <ng-content select="[page-actions]" />
      </header>
      <ng-content />
    </div>
  `,
  styles: [`
    .page {
      max-width: 1200px;
      padding: 1.5rem;
      margin: 0 auto;
    }
    .page__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 1.5rem;
      gap: 1rem;
    }
    .page__header-left {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .page__icon {
      color: var(--color-primary);
      flex-shrink: 0;
    }
    .page__title {
      font-size: 1.5rem;
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
    }
    .page__subtitle {
      font-size: 0.875rem;
      color: var(--text-muted);
      margin-top: 0.25rem;
    }
    @media (max-width: 767px) {
      .page { padding: 0.75rem; }
      .page__header { flex-direction: column; align-items: stretch; }
      .page__title { font-size: 1.25rem; }
    }
  `]
})
export class PageComponent {
  title = input('');
  subtitle = input('');
  icon = input<string | null>(null);
}
