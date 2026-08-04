import { Component, input, output } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-section-header',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './section-header.component.html',
  styleUrl: './section-header.component.scss'
})
export class SectionHeaderComponent {
  title = input('');
  subtitle = input('');
  icon = input<string | null>(null);
  showAdd = input(true);
  addLabel = input('Novo');
  add = output<void>();
}
