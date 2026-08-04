import { Component, input } from '@angular/core';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  templateUrl: './skeleton.component.html',
  styleUrl: './skeleton.component.scss'
})
export class SkeletonComponent {
  type = input<'card' | 'row' | 'text'>('text');
  count = input(3);
  height = input('1rem');
}
