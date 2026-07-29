import { Component, input } from '@angular/core';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  template: `
    <div class="skeleton-group">
      @for (_ of [].constructor(count()); track $index) {
        @switch (type()) {
          @case ('card') {
            <div class="skeleton skeleton--card" [style.height]="height()"></div>
          }
          @case ('row') {
            <div class="skeleton skeleton--row">
              <div class="skeleton__line" [style.width]="'60%'"></div>
              <div class="skeleton__line" [style.width]="'30%'"></div>
              <div class="skeleton__line" [style.width]="'20%'"></div>
            </div>
          }
          @default {
            <div class="skeleton skeleton--line" [style.height]="height()"></div>
          }
        }
      }
    </div>
  `,
  styles: [`
    .skeleton-group { display: flex; flex-direction: column; gap: 0.75rem; }
    .skeleton {
      background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%);
      background-size: 200% 100%; animation: shimmer 1.5s infinite; border-radius: var(--radius-md);
    }
    .skeleton--card { width: 100%; min-height: 100px; }
    .skeleton--row { padding: 1rem; background: var(--content-surface); border: 1px solid var(--surface-border); display: flex; flex-direction: column; gap: 0.5rem; }
    .skeleton--line { width: 100%; height: 1rem; }
    .skeleton__line { height: 0.75rem; background: #e2e8f0; border-radius: var(--radius-sm); }
    @keyframes shimmer { 0% { background-position: -200% 0; } 100% { background-position: 200% 0; } }
  `]
})
export class SkeletonComponent {
  type = input<'card' | 'row' | 'text'>('text');
  count = input(3);
  height = input('1rem');
}
