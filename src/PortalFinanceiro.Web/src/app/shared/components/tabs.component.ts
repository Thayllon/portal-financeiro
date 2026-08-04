import { Component, input, output } from '@angular/core';

export interface Tab {
  id: string;
  label: string;
}

@Component({
  selector: 'app-tabs',
  standalone: true,
  templateUrl: './tabs.component.html',
  styleUrl: './tabs.component.scss'
})
export class TabsComponent {
  tabs = input<Tab[]>([]);
  active = input<string>('');
  change = output<string>();
}
