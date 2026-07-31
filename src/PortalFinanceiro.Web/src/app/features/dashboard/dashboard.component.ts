import { Component, inject, signal, OnInit } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DashboardRepository } from '../../core/repositories/dashboard.repository';
import { Dashboard, PrevisaoMensal } from '../../core/models/dashboard.model';
import { NotificationService } from '../../core/services/notification.service';
import { SkeletonComponent } from '../../shared/components/skeleton.component';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { CurrencyBRLPipe } from '../../shared/pipes/currency-brl.pipe';

const MESES = ['Janeiro','Fevereiro','Março','Abril','Maio','Junho','Julho','Agosto','Setembro','Outubro','Novembro','Dezembro'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [SkeletonComponent, MonthNavComponent, StatusBadgeComponent, CurrencyBRLPipe],
  template: `
    <div class="page">
      <div class="page-header">
        <h1>Dashboard</h1>
      </div>

      <div class="month-area">
        <app-month-nav [mes]="mes()" [ano]="ano()" (prev)="navegarMes(-1)" (next)="navegarMes(1)" />
      </div>

      @if (loading()) {
        <div class="cards">
          <app-skeleton type="card" [count]="4" />
        </div>
      } @else {
        <div class="cards">
          <div class="card">
            <div class="card-label">Receitas</div>
            <div class="card-value green">{{ data()?.totalReceitas | currencyBRL }}</div>
            <div class="card-sub">
              <span class="pill">{{ data()?.totalRecebido | currencyBRL }} recebido</span>
            </div>
          </div>
          <div class="card">
            <div class="card-label">Despesas</div>
            <div class="card-value red">{{ data()?.totalDespesas | currencyBRL }}</div>
            <div class="card-sub">
              <span class="pill green">{{ data()?.totalPago | currencyBRL }} pago</span>
            </div>
          </div>
          <div class="card">
            <div class="card-label">Saldo previsto</div>
            <div class="card-value" [class.green]="(data()?.saldo ?? 0) >= 0" [class.red]="(data()?.saldo ?? 0) < 0">
              {{ data()?.saldo | currencyBRL }}
            </div>
            <div class="card-sub">receitas - despesas</div>
          </div>
          <div class="card">
            <div class="card-label">Saldo realizado</div>
            <div class="card-value" [class.green]="(data()?.saldoRealizado ?? 0) >= 0" [class.red]="(data()?.saldoRealizado ?? 0) < 0">
              {{ data()?.saldoRealizado | currencyBRL }}
            </div>
            <div class="card-sub">recebido - pago</div>
          </div>
        </div>
      }

      @if (data()?.resumoPorConta?.length) {
        <div class="section">
          <h2>Por conta</h2>
          <div class="table-card">
            <table class="table">
              <thead><tr><th>Conta</th><th>Tipo</th><th>Receitas</th><th>Despesas</th><th>Saldo</th></tr></thead>
              <tbody>
                @for (c of data()!.resumoPorConta; track c.nomeConta) {
                  <tr>
                    <td class="cell-name">{{ c.nomeConta }} <span class="cell-meta">{{ c.banco }}</span></td>
                    <td><app-status-badge [type]="c.tipo === 'Pf' ? 'ativo' : 'inativo'" [label]="c.tipo === 'Pf' ? 'PF' : 'PJ'" /></td>
                    <td class="cell-value green">{{ c.totalReceitas | currencyBRL }}</td>
                    <td class="cell-value red">{{ c.totalDespesas | currencyBRL }}</td>
                    <td class="cell-value" [class.green]="c.saldo >= 0" [class.red]="c.saldo < 0">{{ c.saldo | currencyBRL }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }

      @if (data()?.resumoPorCategoria?.length) {
        <div class="section">
          <h2>Por categoria</h2>
          <div class="table-card">
            <table class="table">
              <thead><tr><th>Categoria</th><th>Total</th></tr></thead>
              <tbody>
                @for (c of data()!.resumoPorCategoria; track c.nome) {
                  <tr>
                    <td class="cell-name">{{ c.nome }}</td>
                    <td class="cell-value red">{{ c.total | currencyBRL }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }

      @if (data()?.previsaoProximosMeses?.length) {
        <div class="section">
          <h2>Previsão próximos meses</h2>
          <div class="previsao-cards">
            @for (p of data()!.previsaoProximosMeses; track p.mes + '' + p.ano) {
              <div class="card">
                <div class="card-label">{{ MESES[p.mes - 1] }}/{{ p.ano }}</div>
                <div class="card-value" [class.green]="p.saldoPrevisto >= 0" [class.red]="p.saldoPrevisto < 0">
                  {{ p.saldoPrevisto | currencyBRL }}
                </div>
                <div class="card-sub">
                  {{ p.totalReceitas | currencyBRL }} / {{ p.totalDespesas | currencyBRL }}
                </div>
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1200px; }
    .page-header { margin-bottom: 1rem; }
    h1 { font-size: 1.5rem; font-weight: 600; color: var(--text-primary); }
    h2 { font-size: 1.125rem; font-weight: 600; color: var(--text-primary); margin-bottom: 1rem; }
    .month-area { margin-bottom: 1.5rem; }
    .cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1rem; }
    .card { background: var(--content-surface); border: 1px solid var(--surface-border); border-radius: var(--radius-lg); padding: 1.25rem; }
    .card-label { font-size: 0.8125rem; color: var(--text-muted); margin-bottom: 0.5rem; text-transform: uppercase; letter-spacing: 0.03em; }
    .card-value { font-size: 1.5rem; font-weight: 700; color: var(--text-primary); }
    .card-value.green { color: var(--color-success); }
    .card-value.red { color: var(--color-error); }
    .card-sub { font-size: 0.75rem; color: var(--text-muted); margin-top: 0.5rem; display: flex; gap: 0.5rem; align-items: center; }
    .pill { display: inline-flex; padding: 0.125rem 0.5rem; border-radius: 999px; font-weight: 500; }
    .pill.green { background: var(--color-success-bg); color: var(--color-success); }
    .section { margin-top: 2rem; }
    .previsao-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; }
    .table-card { background: var(--content-surface); border: 1px solid var(--surface-border); border-radius: var(--radius-lg); overflow: hidden; }
    .table { width: 100%; border-collapse: collapse; }
    .table th { text-align: left; padding: 0.75rem 1rem; font-size: 0.75rem; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em; border-bottom: 1px solid var(--surface-border); }
    .table td { padding: 0.75rem 1rem; font-size: 0.875rem; color: var(--text-primary); border-bottom: 1px solid var(--surface-border); }
    .table tr:last-child td { border-bottom: none; }
    .table tr:hover { background: var(--surface-hover); }
    .cell-name { font-weight: 500; }
    .cell-value { font-variant-numeric: tabular-nums; white-space: nowrap; }
    .cell-meta { color: var(--text-muted); font-size: 0.8125rem; margin-left: 0.5rem; }
    .cell-value.green { color: var(--color-success); }
    .cell-value.red { color: var(--color-error); }
  `]
})
export class DashboardComponent implements OnInit {
  private repo = inject(DashboardRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);

  data = signal<Dashboard | null>(null);
  loading = signal(true);
  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());

  readonly MESES = MESES;

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      this.data.set(await firstValueFrom(this.repo.obter(this.auth.user()!.usuarioId, this.mes(), this.ano())));
    } catch { this.notify.error('Erro ao carregar dashboard'); }
    finally { this.loading.set(false); }
  }

  navegarMes(dir: number) {
    let m = this.mes() + dir, a = this.ano();
    if (m > 12) { m = 1; a++; }
    if (m < 1) { m = 12; a--; }
    this.mes.set(m); this.ano.set(a);
    this.carregar();
  }
}
