import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="page">
      <div class="page-header">
        <h1>Dashboard</h1>
        <p class="subtitle">Visão geral das suas finanças</p>
      </div>
      <div class="cards">
        <div class="card">
          <div class="card-label">Receitas do mês</div>
          <div class="card-value green">R$ 0,00</div>
          <div class="card-sub">0% recebidas</div>
        </div>
        <div class="card">
          <div class="card-label">Despesas do mês</div>
          <div class="card-value red">R$ 0,00</div>
          <div class="card-sub">0% pagas</div>
        </div>
        <div class="card">
          <div class="card-label">Saldo</div>
          <div class="card-value">R$ 0,00</div>
          <div class="card-sub">previsto</div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page { max-width: 1200px; }

    .page-header {
      margin-bottom: 1.5rem;
    }

    h1 {
      font-size: 1.5rem;
      font-weight: 600;
      color: #0f172a;
    }

    .subtitle {
      font-size: 0.875rem;
      color: #64748b;
      margin-top: 0.25rem;
    }

    .cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1rem;
    }

    .card {
      background: #fff;
      border-radius: 8px;
      padding: 1.25rem;
      border: 1px solid #e2e8f0;
    }

    .card-label {
      font-size: 0.8125rem;
      color: #64748b;
      margin-bottom: 0.5rem;
    }

    .card-value {
      font-size: 1.5rem;
      font-weight: 600;
      color: #0f172a;
    }

    .card-value.green { color: #16a34a; }
    .card-value.red { color: #dc2626; }

    .card-sub {
      font-size: 0.75rem;
      color: #94a3b8;
      margin-top: 0.25rem;
    }
  `]
})
export class DashboardComponent {}
