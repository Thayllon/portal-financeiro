import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  templateUrl: './empty-state.component.html',
  styleUrl: './empty-state.component.scss'
})
export class EmptyStateComponent {
  title = input('');
  description = input('');
  showAction = input(true);
  actionLabel = input('Começar');
  action = output<void>();
}
