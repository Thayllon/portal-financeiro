import { Component } from '@angular/core';
import { PessoaListagemComponent } from '../pessoas/pessoa-listagem.component';
import { TipoPessoa } from '../../core/models/enums';

@Component({
  selector: 'app-parceiros',
  standalone: true,
  imports: [PessoaListagemComponent],
  template: `<app-pessoa-listagem [tipo]="TipoPessoa.Parceiro" />`
})
export class ParceirosComponent {
  readonly TipoPessoa = TipoPessoa;
}
