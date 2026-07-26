import { Component } from '@angular/core';

@Component({
  selector: 'app-receitas',
  standalone: true,
  template: `<div class="page"><h1>Receitas</h1><p>Gestão de receitas recorrentes.</p></div>`,
  styles: [`.page { max-width: 1200px; } h1 { margin-bottom: 1rem; color: #1a1a2e; }`]
})
export class ReceitasComponent {}
