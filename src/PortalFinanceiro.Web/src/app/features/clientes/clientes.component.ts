import { Component } from '@angular/core';
import { PessoaListagemComponent } from '../pessoas/pessoa-listagem.component';
import { TipoPessoa } from '../../core/models/enums';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [PessoaListagemComponent],
  template: `<app-pessoa-listagem [tipo]="TipoPessoa.Cliente" />`
})
export class ClientesComponent {
  readonly TipoPessoa = TipoPessoa;
}
