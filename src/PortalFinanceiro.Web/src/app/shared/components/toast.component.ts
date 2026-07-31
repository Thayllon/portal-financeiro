import { Component, inject } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [LucideDynamicIcon],
  template: `
    <div class="toast-container">
      @for (t of notification.toasts(); track t.id) {
        <div class="toast toast--{{ t.type }}" (click)="notification.dismiss(t.id)">
          @switch (t.type) {
            @case ('success') { <svg lucideIcon="check-circle" [size]="16" class="toast__icon" /> }
            @case ('error') { <svg lucideIcon="alert-circle" [size]="16" class="toast__icon" /> }
            @case ('info') { <svg lucideIcon="info" [size]="16" class="toast__icon" /> }
          }
          <span>{{ t.message }}</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 1rem;
      right: 1rem;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      max-width: 380px;
    }
    .toast {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.75rem 1rem;
      border-radius: var(--radius-lg);
      font-size: 0.875rem;
      cursor: pointer;
      box-shadow: var(--shadow-lg);
      animation: slideInRight 0.2s ease;
      will-change: transform, opacity;
    }
    .toast__icon { flex-shrink: 0; }
    .toast--success { background: var(--color-success-bg); color: var(--color-success); border: 1px solid var(--color-success); }
    .toast--error { background: var(--color-error-bg); color: var(--color-error); border: 1px solid var(--color-error); }
    .toast--info { background: var(--color-info-bg); color: var(--color-info); border: 1px solid var(--color-info); }
    @keyframes slideInRight { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
  `]
})
export class ToastComponent {
  notification = inject(NotificationService);
}
