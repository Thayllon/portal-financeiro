import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmService } from '../services/confirm.service';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [FormsModule, LucideDynamicIcon],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss'
})
export class ConfirmDialogComponent {
  confirm = inject(ConfirmService);

  texto = signal('');

  podeConfirmar = computed(() => {
    const chave = this.confirm.palavraChave();
    if (chave === null) return true;
    return this.texto().trim().toLowerCase() === chave.toLowerCase();
  });

  constructor() {
    effect(() => {
      if (!this.confirm.visible()) this.texto.set('');
    });
  }
}
