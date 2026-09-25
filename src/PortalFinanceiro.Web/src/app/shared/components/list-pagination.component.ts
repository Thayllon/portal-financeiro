import { Component, input, computed } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';
import { PaginationState } from '../composables/use-list-pagination.composable';
import { PAGE_SIZE_OPTIONS, PageSizeOption } from '../constants/pagination.constants';
import { CustomSelectComponent, SelectOption } from './custom-select.component';

@Component({
  selector: 'app-list-pagination',
  standalone: true,
  imports: [LucideDynamicIcon, CustomSelectComponent],
  templateUrl: './list-pagination.component.html',
  styleUrl: './list-pagination.component.scss'
})
export class ListPaginationComponent {
  pagination = input.required<PaginationState & { pageSize: import('@angular/core').Signal<PageSizeOption>; onPageSizeChange: (size: PageSizeOption) => void }>();

  sizeOptions: SelectOption[] = PAGE_SIZE_OPTIONS.map(size => ({ value: String(size), label: String(size) }));
  tamanhoSelecionado = computed(() => String(this.pagination().pageSize()));

  onSizeChangeValue(value: string) {
    this.pagination().onPageSizeChange(Number(value) as PageSizeOption);
  }
}
