import { Component } from '@angular/core';
import { LancamentoListagemComponent } from '../lancamentos/lancamento-listagem.component';

@Component({
  selector: 'app-despesas',
  standalone: true,
  imports: [LancamentoListagemComponent],
  template: `<app-lancamento-listagem tipo="despesa" />`
})
export class DespesasComponent {}