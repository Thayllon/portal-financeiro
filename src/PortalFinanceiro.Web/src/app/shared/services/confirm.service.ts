import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private _resolve: ((value: boolean) => void) | null = null;

  visible = signal(false);
  title = signal('');
  message = signal('');

  confirm(title: string, message: string): Promise<boolean> {
    this.title.set(title);
    this.message.set(message);
    this.visible.set(true);
    return new Promise(resolve => { this._resolve = resolve; });
  }

  accept() {
    this.visible.set(false);
    this._resolve?.(true);
  }

  cancel() {
    this.visible.set(false);
    this._resolve?.(false);
  }
}
