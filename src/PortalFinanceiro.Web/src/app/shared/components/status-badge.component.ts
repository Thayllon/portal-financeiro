import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss'
})
export class StatusBadgeComponent {
  type = input<'pendente' | 'realizado' | 'ativo' | 'inativo'>('pendente');
  label = input('');
}
