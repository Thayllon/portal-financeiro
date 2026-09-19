import { Component, inject, signal, OnInit, computed, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DashboardRepository } from '../../core/repositories/dashboard.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { Dashboard, DashboardAnual, DistribuicaoCategoriaAnual } from '../../core/models/dashboard.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';
import { SkeletonComponent } from '../../shared/components/skeleton.component';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { CurrencyBRLPipe } from '../../shared/pipes/currency-brl.pipe';
import { CustomSelectComponent } from '../../shared/components/custom-select.component';
import { CollapsibleSectionComponent } from '../../shared/components/collapsible-section.component';
import { LucideDynamicIcon } from '@lucide/angular';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, Chart, registerables } from 'chart.js';

Chart.register(...registerables);

const MESES = ['Janeiro','Fevereiro','Março','Abril','Maio','Junho','Julho','Agosto','Setembro','Outubro','Novembro','Dezembro'];

const COR_GRAFICO_RECEITA = '#16a34a';
const COR_GRAFICO_RECEITA_BG = '#16a34acc';
const COR_GRAFICO_DESPESA = '#dc2626';
const COR_GRAFICO_DESPESA_BG = '#dc2626cc';

const PALETA_DONUT = ['#0d9488', '#dc2626', '#5b8def', '#eab308', '#f97316', '#a855f7', '#64748b', '#16a34a', '#ec4899', '#14b8a6'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, SkeletonComponent, MonthNavComponent, StatusBadgeComponent, CurrencyBRLPipe, CustomSelectComponent, CollapsibleSectionComponent, LucideDynamicIcon, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private repo = inject(DashboardRepository);
  private contaRepo = inject(ContaBancariaRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);

  data = signal<Dashboard | null>(null);
  dataAnual = signal<DashboardAnual | null>(null);
  loading = signal(true);
  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());
  visualizacao = signal<'mensal' | 'anual'>('mensal');
  filtroConta = signal<string>('');
  contas = signal<ContaBancaria[]>([]);
  tipoDistribuicao = signal<'receita' | 'despesa'>('despesa');
  nivelDistribuicao = signal<'categoria' | 'subcategoria'>('categoria');

  readonly MESES = MESES;

  exibirPrevisao = computed(() => {
    const hoje = new Date();
    return this.ano() > hoje.getFullYear() || (this.ano() === hoje.getFullYear() && this.mes() >= hoje.getMonth() + 1);
  });

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
    const contas = this.contas();
    return contas.filter(c => c.ativo).map(c => ({ value: c.id, label: `${c.nome} (${c.banco})` }));
  });

  distribuicaoAtual = computed<DistribuicaoCategoriaAnual[]>(() => {
    const anual = this.dataAnual();
    if (!anual) return [];
    const base = this.tipoDistribuicao() === 'receita' ? anual.distribuicaoReceitas : anual.distribuicaoDespesas;
    if (this.nivelDistribuicao() === 'categoria') return base;
    const flat: DistribuicaoCategoriaAnual[] = [];
    for (const cat of base) {
      for (const sub of cat.subcategorias ?? []) {
        flat.push({ nome: `${cat.nome} • ${sub.nome}`, total: sub.total, percentual: sub.percentual, subcategorias: [] });
      }
    }
    return flat.sort((a, b) => b.total - a.total);
  });

  totalDistribuicao = computed(() => this.distribuicaoAtual().reduce((s, i) => s + i.total, 0));

  mesInicialPrevisao = computed(() => {
    const anual = this.dataAnual();
    if (!anual?.previsaoRestanteAno?.length) return '';
    const meses = ['janeiro','fevereiro','março','abril','maio','junho','julho','agosto','setembro','outubro','novembro','dezembro'];
    return meses[anual.previsaoRestanteAno[0].mes - 1];
  });

  doughnutChartData: ChartConfiguration<'doughnut'>['data'] = {
    labels: [],
    datasets: []
  };

  doughnutChartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (context) => {
            const value = context.parsed ?? 0;
            return `${context.label}: R$ ${Number(value).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`;
          }
        }
      }
    }
  };

  private requestSeq = 0;
  chartVersion = signal(0);
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;
  private graficoMensal = signal<{ label: string; d: Dashboard }[]>([]);

  ngOnInit() { this.carregar(); this.carregarContas(); }

  async carregarContas() {
    try {
      const contas = await firstValueFrom(this.contaRepo.listar());
      this.contas.set(contas ?? []);
    } catch { this.contas.set([]); }
  }

  async carregar() {
    const seq = ++this.requestSeq;
    this.loading.set(true);
    try {
      if (this.visualizacao() === 'mensal') {
        const mesAtual = this.mes();
        const anoAtual = this.ano();
        const meses = [
          this.mesAdjacente(mesAtual, anoAtual, -1),
          { mes: mesAtual, ano: anoAtual },
          this.mesAdjacente(mesAtual, anoAtual, 1),
        ];
        const dashboards = await Promise.all(
          meses.map(m => firstValueFrom(this.repo.obter(m.mes, m.ano)))
        );
if (seq !== this.requestSeq) return;
            this.data.set(dashboards[1]);
            this.graficoMensal.set(meses.map((m, i) => ({ label: MESES[m.mes - 1], d: dashboards[i] })));
            this.atualizarGraficoMensal();
            this.chartVersion.update(v => v + 1);
            setTimeout(() => this.chart?.update(), 50);
      } else {
        const idConta = this.filtroConta() || undefined;
        const anual = await firstValueFrom(this.repo.obterAnual(this.ano(), idConta));
if (seq !== this.requestSeq) return;
            this.dataAnual.set(anual);
            this.atualizarGraficoAnual();
            this.atualizarDonut();
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
    this.graficoMensal.set([]);
    this.barChartData = { labels: [], datasets: [] };
    this.doughnutChartData = { labels: [], datasets: [] };
    this.carregar();
  }

  onFiltroContaChange(valor: string) {
    this.filtroConta.set(valor);
    if (this.visualizacao() === 'anual') {
      this.carregar();
    }
  }

  trocarTipoDistribuicao(tipo: 'receita' | 'despesa') {
    this.tipoDistribuicao.set(tipo);
    this.atualizarDonut();
  }

  trocarNivelDistribuicao(nivel: 'categoria' | 'subcategoria') {
    this.nivelDistribuicao.set(nivel);
    this.atualizarDonut();
  }

  private atualizarGraficoMensal() {
    const itens = this.graficoMensal();
    if (!itens.length) return;

    this.barChartData = {
      labels: itens.map(i => i.label),
      datasets: [
        {
          data: itens.map(i => i.d.totalReceitas),
          label: 'Receitas',
          backgroundColor: COR_GRAFICO_RECEITA_BG,
          borderColor: COR_GRAFICO_RECEITA,
          borderWidth: 1
        },
        {
          data: itens.map(i => i.d.totalDespesas),
          label: 'Despesas',
          backgroundColor: COR_GRAFICO_DESPESA_BG,
          borderColor: COR_GRAFICO_DESPESA,
          borderWidth: 1
        }
      ]
    };
  }

  private mesAdjacente(mes: number, ano: number, delta: number): { mes: number; ano: number } {
    let m = mes + delta;
    let a = ano;
    while (m > 12) { m -= 12; a++; }
    while (m < 1) { m += 12; a--; }
    return { mes: m, ano: a };
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
          backgroundColor: COR_GRAFICO_RECEITA_BG,
          borderColor: COR_GRAFICO_RECEITA,
          borderWidth: 1
        },
        {
          data: anual.resumoPorMes.map(m => m.totalDespesas),
          label: 'Despesas',
          backgroundColor: COR_GRAFICO_DESPESA_BG,
          borderColor: COR_GRAFICO_DESPESA,
          borderWidth: 1
        }
      ]
    };
  }

  private atualizarDonut() {
    const itens = this.distribuicaoAtual();
    this.doughnutChartData = {
      labels: itens.map(i => i.nome),
      datasets: [
        {
          data: itens.map(i => i.total),
          backgroundColor: itens.map((_, idx) => PALETA_DONUT[idx % PALETA_DONUT.length]),
          borderWidth: 2,
          borderColor: '#ffffff'
        }
      ]
    };
  }
}
