import { Component } from '@angular/core';

@Component({
  selector: 'app-categorias-despesa',
  standalone: true,
  template: `<div class="page"><h1>Categorias de Despesa</h1><p>Gestão de categorias.</p></div>`,
  styles: [`.page { max-width: 1200px; } h1 { margin-bottom: 1rem; color: #1a1a2e; }`]
})
export class CategoriasDespesaComponent {}
