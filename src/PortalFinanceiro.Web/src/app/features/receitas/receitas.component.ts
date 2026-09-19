import { Component } from '@angular/core';
import { LancamentoListagemComponent } from '../lancamentos/lancamento-listagem.component';

@Component({
  selector: 'app-receitas',
  standalone: true,
  imports: [LancamentoListagemComponent],
  template: `<app-lancamento-listagem tipo="receita" />`
})
export class ReceitasComponent {}