import { Component, input } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-page',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './page.component.html',
  styleUrl: './page.component.scss'
})
export class PageComponent {
  title = input('');
  subtitle = input('');
  icon = input<string | null>(null);
}
