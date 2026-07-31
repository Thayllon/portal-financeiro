import { Component, input } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';
import { PaginationState } from '../composables/use-list-pagination.composable';
import { PAGE_SIZE_OPTIONS, PageSizeOption } from '../constants/pagination.constants';

@Component({
  selector: 'app-list-pagination',
  standalone: true,
  imports: [LucideDynamicIcon],
  template: `
    @if (pagination().totalItems() > 0) {
      <div class="lp">
        <div class="lp__left">
          <span class="lp__info">{{ pagination().rangeStart() }}-{{ pagination().rangeEnd() }} de {{ pagination().totalItems() }}</span>
          <select class="lp__size" (change)="onSizeChange($event)">
            @for (size of pageSizes; track size) {
              <option [value]="size" [selected]="size === pagination().pageSize()">{{ size }}</option>
            }
          </select>
        </div>
        <div class="lp__controls">
          <button class="lp__btn" [disabled]="pagination().currentPage() <= 1" (click)="pagination().goToPreviousPage()">
            <svg lucideIcon="chevron-left" [size]="14" /> Anterior
          </button>
          <span class="lp__page">Página {{ pagination().currentPage() }} de {{ pagination().totalPages() }}</span>
          <button class="lp__btn" [disabled]="pagination().currentPage() >= pagination().totalPages()" (click)="pagination().goToNextPage()">
            Próximo <svg lucideIcon="chevron-right" [size]="14" />
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .lp {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem 1rem;
      border-top: 1px solid var(--surface-border);
      background: var(--content-surface);
      border-radius: 0 0 var(--radius-xl) var(--radius-xl);
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .lp__left {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .lp__info {
      font-size: 0.8125rem;
      color: var(--text-muted);
    }
    .lp__size {
      padding: 0.25rem 0.5rem;
      border: 1px solid var(--surface-border);
      border-radius: var(--radius-md);
      font-size: 0.8125rem;
      color: var(--text-primary);
      background: var(--content-surface);
      cursor: pointer;
    }
    .lp__controls {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .lp__btn {
      display: inline-flex;
      align-items: center;
      gap: 0.375rem;
      background: none;
      border: 1px solid var(--surface-border);
      border-radius: var(--radius-md);
      padding: 0.375rem 0.75rem;
      font-size: 0.8125rem;
      color: var(--text-primary);
      cursor: pointer;
      transition: background var(--transition-fast);
    }
    .lp__btn:hover:not(:disabled) { background: var(--surface-hover); }
    .lp__btn:disabled { opacity: 0.4; cursor: not-allowed; }
    .lp__page {
      font-size: 0.8125rem;
      color: var(--text-muted);
    }
    @media (max-width: 767px) {
      .lp { flex-direction: column; align-items: stretch; }
      .lp__left, .lp__controls { justify-content: center; }
    }
  `]
})
export class ListPaginationComponent {
  pagination = input.required<PaginationState & { pageSize: import('@angular/core').Signal<PageSizeOption>; onPageSizeChange: (size: PageSizeOption) => void }>();

  pageSizes = PAGE_SIZE_OPTIONS;

  onSizeChange(event: Event) {
    const value = Number((event.target as HTMLSelectElement).value) as PageSizeOption;
    this.pagination().onPageSizeChange(value);
  }
}
