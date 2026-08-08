import { Component, input, signal } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-collapsible-section',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './collapsible-section.component.html',
  styleUrl: './collapsible-section.component.scss'
})
export class CollapsibleSectionComponent {
  title = input.required<string>();
  icon = input.required<string>();
  iconVariant = input<'default' | 'green' | 'red' | 'primary' | 'info'>('default');
  badge = input<string>('');

  collapsed = signal(true);

  toggle() {
    this.collapsed.update(v => !v);
  }
}
