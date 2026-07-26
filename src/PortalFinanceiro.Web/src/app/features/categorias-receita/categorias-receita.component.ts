import { Component } from '@angular/core';

@Component({
  selector: 'app-categorias-receita',
  standalone: true,
  template: `<div class="page"><h1>Categorias de Receita</h1><p>Gestão de categorias.</p></div>`,
  styles: [`.page { max-width: 1200px; } h1 { margin-bottom: 1rem; color: #1a1a2e; }`]
})
export class CategoriasReceitaComponent {}
