import { Component } from '@angular/core';
import { PessoaListagemComponent } from '../pessoas/pessoa-listagem.component';

@Component({
  selector: 'app-parceiros',
  standalone: true,
  imports: [PessoaListagemComponent],
  template: `<app-pessoa-listagem tipo="Parceiro" />`
})
export class ParceirosComponent {}