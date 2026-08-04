import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-month-nav',
  standalone: true,
  templateUrl: './month-nav.component.html',
  styleUrl: './month-nav.component.scss'
})
export class MonthNavComponent {
  mes = input<number>(0);
  ano = input<number>(0);
  prev = output<void>();
  next = output<void>();
}
