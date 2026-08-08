import { Component, inject, signal, OnInit, computed, ViewChild } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DashboardRepository } from '../../core/repositories/dashboard.repository';
import { Dashboard, DashboardAnual } from '../../core/models/dashboard.model';
import { NotificationService } from '../../core/services/notification.service';
import { SkeletonComponent } from '../../shared/components/skeleton.component';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { CurrencyBRLPipe } from '../../shared/pipes/currency-brl.pipe';
import { CustomSelectComponent } from '../../shared/components/custom-select.component';
import { LucideDynamicIcon } from '@lucide/angular';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, Chart, registerables } from 'chart.js';

Chart.register(...registerables);

const MESES = ['Janeiro','Fevereiro','Março','Abril','Maio','Junho','Julho','Agosto','Setembro','Outubro','Novembro','Dezembro'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [SkeletonComponent, MonthNavComponent, StatusBadgeComponent, CurrencyBRLPipe, CustomSelectComponent, LucideDynamicIcon, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private repo = inject(DashboardRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);

  data = signal<Dashboard | null>(null);
  dataAnual = signal<DashboardAnual | null>(null);
  loading = signal(true);
  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());
  visualizacao = signal<'mensal' | 'anual'>('mensal');
  filtroConta = signal<string>('');

  readonly MESES = MESES;

  barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: []
  };

  barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'top' },
      tooltip: {
        callbacks: {
          label: (context) => {
            const value = context.parsed.y ?? 0;
            return `${context.dataset.label}: R$ ${value.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => `R$ ${Number(value).toLocaleString('pt-BR')}`
        }
      }
    }
  };

    contasOptions = computed(() => {
    const contas = this.dataAnual()?.resumoPorConta ?? [];
    return contas.map(c => ({ value: c.nomeConta, label: `${c.nomeConta} (${c.banco})` }));
  });

  private requestSeq = 0;
  chartVersion = signal(0);
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;

  ngOnInit() { this.carregar(); }

  async carregar() {
    const seq = ++this.requestSeq;
    this.loading.set(true);
    try {
      if (this.visualizacao() === 'mensal') {
        const d = await firstValueFrom(this.repo.obter(this.mes(), this.ano()));
if (seq !== this.requestSeq) return;
            this.data.set(d);
            this.atualizarGraficoMensal();
            this.chartVersion.update(v => v + 1);
            setTimeout(() => this.chart?.update(), 50);
      } else {
        const idConta = this.filtroConta() || undefined;
        const anual = await firstValueFrom(this.repo.obterAnual(this.ano(), idConta));
if (seq !== this.requestSeq) return;
            this.dataAnual.set(anual);
            this.atualizarGraficoAnual();
            this.chartVersion.update(v => v + 1);
            setTimeout(() => this.chart?.update(), 50);
      }
    } catch { this.notify.error('Erro ao carregar dashboard'); }
    finally { if (seq === this.requestSeq) this.loading.set(false); }
  }

  navegarMes(dir: number) {
    let m = this.mes() + dir, a = this.ano();
    if (m > 12) { m = 1; a++; }
    if (m < 1) { m = 12; a--; }
    this.mes.set(m); this.ano.set(a);
    this.carregar();
  }

  navegarAno(dir: number) {
    this.ano.set(this.ano() + dir);
    this.carregar();
  }

  trocarVisualizacao(tipo: 'mensal' | 'anual') {
    this.visualizacao.set(tipo);
    this.data.set(null);
    this.dataAnual.set(null);
    this.barChartData = { labels: [], datasets: [] };
    this.carregar();
  }

  onFiltroContaChange(valor: string) {
    this.filtroConta.set(valor);
    if (this.visualizacao() === 'anual') {
      this.carregar();
    }
  }

  private atualizarGraficoMensal() {
    const d = this.data();
    if (!d) return;

    this.barChartData = {
      labels: [MESES[this.mes() - 1]],
      datasets: [
        {
          data: [d.totalReceitas],
          label: 'Receitas',
          backgroundColor: 'rgba(22, 163, 74, 0.8)',
          borderColor: 'rgb(22, 163, 74)',
          borderWidth: 1
        },
        {
          data: [d.totalDespesas],
          label: 'Despesas',
          backgroundColor: 'rgba(220, 38, 38, 0.8)',
          borderColor: 'rgb(220, 38, 38)',
          borderWidth: 1
        }
      ]
    };
  }

  private atualizarGraficoAnual() {
    const anual = this.dataAnual();
    if (!anual) return;

    this.barChartData = {
      labels: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
      datasets: [
        {
          data: anual.resumoPorMes.map(m => m.totalReceitas),
          label: 'Receitas',
          backgroundColor: 'rgba(22, 163, 74, 0.8)',
          borderColor: 'rgb(22, 163, 74)',
          borderWidth: 1
        },
        {
          data: anual.resumoPorMes.map(m => m.totalDespesas),
          label: 'Despesas',
          backgroundColor: 'rgba(220, 38, 38, 0.8)',
          borderColor: 'rgb(220, 38, 38)',
          borderWidth: 1
        }
      ]
    };
  }
}
