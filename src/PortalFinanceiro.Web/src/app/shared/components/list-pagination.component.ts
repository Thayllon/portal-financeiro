import { Component, input } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';
import { PaginationState } from '../composables/use-list-pagination.composable';
import { PAGE_SIZE_OPTIONS, PageSizeOption } from '../constants/pagination.constants';

@Component({
  selector: 'app-list-pagination',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './list-pagination.component.html',
  styleUrl: './list-pagination.component.scss'
})
export class ListPaginationComponent {
  pagination = input.required<PaginationState & { pageSize: import('@angular/core').Signal<PageSizeOption>; onPageSizeChange: (size: PageSizeOption) => void }>();

  pageSizes = PAGE_SIZE_OPTIONS;

  onSizeChange(event: Event) {
    const value = Number((event.target as HTMLSelectElement).value) as PageSizeOption;
    this.pagination().onPageSizeChange(value);
  }
}
