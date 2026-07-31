import { computed, signal, type Signal } from '@angular/core';
import { PAGE_SIZE_OPTIONS, type PageSizeOption } from '../constants/pagination.constants';

export interface PaginationState {
  rangeStart: Signal<number>;
  rangeEnd: Signal<number>;
  totalItems: Signal<number>;
  currentPage: Signal<number>;
  totalPages: Signal<number>;
  goToPreviousPage: () => void;
  goToNextPage: () => void;
}

export function useListPagination<T>(
  items: Signal<T[]>,
  opts?: { initialPageSize?: PageSizeOption }
): PaginationState & { paginatedItems: Signal<T[]>; onPageSizeChange: (size: PageSizeOption) => void; pageSize: Signal<PageSizeOption> } {
  const currentPage = signal(1);
  const pageSize = signal<PageSizeOption>(opts?.initialPageSize ?? PAGE_SIZE_OPTIONS[0]);

  const totalItems = computed(() => items().length);
  const totalPages = computed(() => Math.max(1, Math.ceil(totalItems() / pageSize())));

  const paginatedItems = computed(() => {
    const start = (currentPage() - 1) * pageSize();
    return items().slice(start, start + pageSize());
  });

  const rangeStart = computed(() => Math.min((currentPage() - 1) * pageSize() + 1, totalItems()));
  const rangeEnd = computed(() => Math.min(currentPage() * pageSize(), totalItems()));

  function goToNextPage() {
    if (currentPage() < totalPages()) currentPage.update(v => v + 1);
  }

  function goToPreviousPage() {
    if (currentPage() > 1) currentPage.update(v => v - 1);
  }

  function onPageSizeChange(size: PageSizeOption) {
    pageSize.set(size);
    currentPage.set(1);
  }

  return {
    paginatedItems,
    currentPage: currentPage.asReadonly(),
    totalPages,
    pageSize: pageSize.asReadonly(),
    rangeStart,
    rangeEnd,
    totalItems,
    goToNextPage,
    goToPreviousPage,
    onPageSizeChange,
  };
}
