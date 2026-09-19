import { Component, input, output } from '@angular/core';
import { ModalComponent } from './modal.component';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-fluxo-selector-modal',
  standalone: true,
  imports: [ModalComponent, LucideDynamicIcon],
  templateUrl: './fluxo-selector-modal.component.html',
  styleUrl: './fluxo-selector-modal.component.scss'
})
export class FluxoSelectorModalComponent {
  visible = input(false);
  tipo = input<'receita' | 'despesa'>('receita');

  visibleChange = output<boolean>();
  escolher = output<boolean>();

  fechar() {
    this.visibleChange.emit(false);
  }

  selecionar(contrato: boolean) {
    this.escolher.emit(contrato);
  }
}
