import { Component, inject } from '@angular/core';
import { ConfirmService } from '../services/confirm.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  template: `
    @if (confirm.visible()) {
      <div class="overlay" (click)="confirm.cancel()">
        <div class="dialog" (click)="$event.stopPropagation()">
          <div class="dialog-header">
            <h3>{{ confirm.title() }}</h3>
          </div>
          <div class="dialog-body">
            <p>{{ confirm.message() }}</p>
          </div>
          <div class="dialog-footer">
            <button class="btn btn--ghost" (click)="confirm.cancel()">Cancelar</button>
            <button class="btn btn--danger" (click)="confirm.accept()">Confirmar</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.5); z-index: 9998;
      display: flex; align-items: center; justify-content: center; padding: 1rem;
    }
    .dialog {
      background: var(--content-surface); border-radius: var(--radius-xl);
      box-shadow: var(--shadow-lg); max-width: 400px; width: 100%; padding: 1.5rem;
    }
    .dialog-header h3 { font-size: 1.125rem; font-weight: 600; color: var(--text-primary); }
    .dialog-body { margin: 1rem 0; }
    .dialog-body p { font-size: 0.875rem; color: var(--text-secondary); line-height: 1.5; }
    .dialog-footer { display: flex; justify-content: flex-end; gap: 0.5rem; }
    .btn {
      padding: 0.5rem 1rem; border-radius: var(--radius-md); font-size: 0.875rem;
      font-weight: 500; transition: all var(--transition-fast);
    }
    .btn--ghost { background: transparent; border: 1px solid var(--surface-border); color: var(--text-secondary); }
    .btn--ghost:hover { background: var(--surface-hover); }
    .btn--danger { background: var(--color-error); border: 1px solid var(--color-error); color: #fff; }
    .btn--danger:hover { opacity: 0.9; }
  `]
})
export class ConfirmDialogComponent {
  confirm = inject(ConfirmService);
}
