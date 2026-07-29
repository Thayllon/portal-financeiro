import { Component, inject } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  template: `
    <div class="toast-container">
      @for (t of notification.toasts(); track t.id) {
        <div class="toast toast--{{ t.type }}" (click)="notification.dismiss(t.id)">
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
      padding: 0.75rem 1rem;
      border-radius: var(--radius-lg);
      font-size: 0.875rem;
      cursor: pointer;
      box-shadow: var(--shadow-lg);
      animation: slideIn 0.2s ease;
    }
    .toast--success { background: var(--color-success-bg); color: var(--color-success); border: 1px solid var(--color-success); }
    .toast--error { background: var(--color-error-bg); color: var(--color-error); border: 1px solid var(--color-error); }
    .toast--info { background: var(--color-info-bg); color: var(--color-info); border: 1px solid var(--color-info); }
    @keyframes slideIn { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
  `]
})
export class ToastComponent {
  notification = inject(NotificationService);
}
