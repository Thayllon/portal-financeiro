import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="page">
      <h1>Dashboard</h1>
      <p>Visão geral do mês será exibida aqui.</p>
    </div>
  `,
  styles: [`
    .page { max-width: 1200px; }
    h1 { margin-bottom: 1rem; color: #1a1a2e; }
  `]
})
export class DashboardComponent {}
