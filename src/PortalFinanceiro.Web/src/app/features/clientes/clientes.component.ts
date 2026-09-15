import { Component } from '@angular/core';
import { PessoaListagemComponent } from '../pessoas/pessoa-listagem.component';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [PessoaListagemComponent],
  template: `<app-pessoa-listagem tipo="Cliente" />`
})
export class ClientesComponent {}