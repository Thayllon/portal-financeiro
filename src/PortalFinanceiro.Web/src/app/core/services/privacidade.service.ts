import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class PrivacidadeService {
  private readonly STORAGE_KEY = 'portal-financeiro.privacidade';

  private ocultoSignal = signal(this.carregar());
  valoresOcultos = this.ocultoSignal.asReadonly();

  toggle() {
    this.definir(!this.ocultoSignal());
  }

  definir(oculto: boolean) {
    this.ocultoSignal.set(oculto);
    try {
      localStorage.setItem(this.STORAGE_KEY, oculto ? '1' : '0');
    } catch {}
  }

  private carregar(): boolean {
    try {
      return localStorage.getItem(this.STORAGE_KEY) === '1';
    } catch {
      return false;
    }
  }
}
