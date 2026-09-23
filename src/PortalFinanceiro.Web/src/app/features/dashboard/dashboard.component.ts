import { Component, inject, signal, OnInit, computed, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DashboardRepository } from '../../core/repositories/dashboard.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { ContratoRepository } from '../../core/repositories/contrato.repository';
import { Contrato } from '../../core/models/contrato.model';
import { Dashboard, DashboardAnual, DistribuicaoCategoriaAnual, ResumoPorConta } from '../../core/models/dashboard.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';
import { SkeletonComponent } from '../../shared/components/skeleton.component';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ValorMascaradoPipe } from '../../shared/pipes/valor-mascarado.pipe';
import { PrivacidadeToggleComponent } from '../../shared/components/privacidade-toggle.component';
import { PrivacidadeService } from '../../core/services/privacidade.service';
import { CustomSelectComponent } from '../../shared/components/custom-select.component';
import { CollapsibleSectionComponent } from '../../shared/components/collapsible-section.component';
import { LucideDynamicIcon } from '@lucide/angular';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, Chart, registerables, Plugin } from 'chart.js';

Chart.register(...registerables);

const MESES = ['Janeiro','Fevereiro','Março','Abril','Maio','Junho','Julho','Agosto','Setembro','Outubro','Novembro','Dezembro'];

function lerCorToken(nome: string, padrao: string): string {
  if (typeof document === 'undefined') return padrao;
  return getComputedStyle(document.documentElement).getPropertyValue(nome).trim() || padrao;
}

const corReceita = (): string => lerCorToken('--color-success', '#16a34a');
const corDespesa = (): string => lerCorToken('--color-error', '#dc2626');
const corRoxa = (): string => lerCorToken('--color-purple', '#7c3aed');
const corTextoGrafico = (): string => lerCorToken('--text-secondary', '#475569');
const corGradeGrafico = (): string => lerCorToken('--surface-border', '#e2e8f0');
const comTransparencia = (cor: string): string => `${cor}cc`;

const TOTAL_VARIANTES_AVATAR = 5;

function iniciaisBanco(banco: string): string {
  const palavras = (banco ?? '').trim().split(/\s+/).filter(Boolean);
  if (palavras.length === 0) return '—';
  if (palavras.length === 1) return palavras[0].slice(0, 2).toUpperCase();
  return `${palavras[0][0]}${palavras[1][0]}`.toUpperCase();
}

function varianteAvatarBanco(banco: string): number {
  const nome = (banco ?? '').trim();
  let hash = 0;
  for (const ch of nome) hash = (hash * 31 + ch.charCodeAt(0)) % TOTAL_VARIANTES_AVATAR;
  return hash;
}

function calcularVariacao(atual: number, anterior: number): number | null {
  if (anterior === 0) return atual === 0 ? 0 : null;
  return Math.round((atual - anterior) / Math.abs(anterior) * 100);
}

function diasReferencia(mes: number, ano: number): number {
  const hoje = new Date();
  if (ano === hoje.getFullYear() && mes === hoje.getMonth() + 1) return hoje.getDate();
  return new Date(ano, mes, 0).getDate();
}

function criarRotuloValorBarras(casasDecimais: number): Plugin<'bar'> {
  return {
    id: 'rotuloValorBarras',
    afterDatasetsDraw(chart) {
      const { ctx } = chart;
      ctx.save();
      ctx.font = "600 11px 'Inter', sans-serif";
      ctx.fillStyle = corTextoGrafico();
      ctx.textAlign = 'center';
      chart.data.datasets.forEach((dataset, datasetIndex) => {
        if (!chart.isDatasetVisible(datasetIndex)) return;
        const meta = chart.getDatasetMeta(datasetIndex);
        meta.data.forEach((elemento, indice) => {
          const props = elemento.getProps(['x', 'y'], true);
          const valor = Number(dataset.data[indice] ?? 0);
          ctx.fillText(
            `R$ ${valor.toLocaleString('pt-BR', { minimumFractionDigits: casasDecimais, maximumFractionDigits: casasDecimais })}`,
            Number(props['x']),
            Number(props['y']) - 6
          );
        });
      });
      ctx.restore();
    }
  };
}

const PALETA_DONUT = ['#0d9488', '#dc2626', '#5b8def', '#eab308', '#f97316', '#a855f7', '#64748b', '#16a34a', '#ec4899', '#14b8a6'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, SkeletonComponent, MonthNavComponent, StatusBadgeComponent, ValorMascaradoPipe, PrivacidadeToggleComponent, CustomSelectComponent, CollapsibleSectionComponent, LucideDynamicIcon, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private repo = inject(DashboardRepository);
  private contaRepo = inject(ContaBancariaRepository);
  private contratoRepo = inject(ContratoRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  protected privacidade = inject(PrivacidadeService);

  data = signal<Dashboard | null>(null);
  dataAnual = signal<DashboardAnual | null>(null);
  loading = signal(true);
  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());
  visualizacao = signal<'mensal' | 'anual'>('mensal');
  filtroConta = signal<string>('');
  contas = signal<ContaBancaria[]>([]);
  contratos = signal<Contrato[]>([]);
  tipoDistribuicao = signal<'receita' | 'despesa'>('despesa');
  nivelDistribuicao = signal<'categoria' | 'subcategoria'>('categoria');

  readonly MESES = MESES;
  readonly iniciaisBanco = iniciaisBanco;
  readonly varianteAvatarBanco = varianteAvatarBanco;

  contasTabela = computed(() => {
    const contas = this.data()?.resumoPorConta ?? [];
    const totalReceitas = contas.reduce((s, c) => s + c.totalReceitas, 0);
    const totalDespesas = contas.reduce((s, c) => s + c.totalDespesas, 0);
    return {
      linhas: contas.map(c => ({
        ...c,
        percentual: totalReceitas > 0 ? Math.round(c.totalReceitas / totalReceitas * 1000) / 10 : 0
      })),
      totalReceitas,
      totalDespesas,
      totalLucro: totalReceitas - totalDespesas
    };
  });

  resumoContratos = computed(() => {
    const ativos = this.contratos().filter(c => c.ativo);
    return {
      qtd: ativos.length,
      faltaReceber: ativos.reduce((s, c) => s + (c.faltaReceber ?? 0), 0)
    };
  });

  recRecorrente = computed(() => {
    const d = this.data();
    const valor = d?.totalReceitasRecorrentes ?? 0;
    const total = d?.totalReceitas ?? 0;
    return { valor, percentual: total > 0 ? Math.round(valor / total * 1000) / 10 : null };
  });

  despRecorrente = computed(() => {
    const d = this.data();
    const valor = d?.totalDespesasRecorrentes ?? 0;
    const total = d?.totalDespesas ?? 0;
    return { valor, percentual: total > 0 ? Math.round(valor / total * 1000) / 10 : null };
  });

  barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: []
  };

  barChartOptions = computed<ChartConfiguration<'bar'>['options']>(() => {
    const oculto = this.privacidade.valoresOcultos();
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { position: 'top' },
        tooltip: {
          callbacks: {
            label: (context) => {
              if (oculto) return '••••••';
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
            callback: (value) => oculto ? '••••••' : `R$ ${Number(value).toLocaleString('pt-BR')}`
          }
        }
      }
    };
  });

  barChartMensalOptions = computed<ChartConfiguration<'bar'>['options']>(() => {
    const oculto = this.privacidade.valoresOcultos();
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top',
          align: 'center',
          labels: {
            usePointStyle: true,
            pointStyle: 'circle',
            boxWidth: 6,
            boxHeight: 6,
            padding: 16,
            color: corTextoGrafico()
          }
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              if (oculto) return '••••••';
              const value = context.parsed.y ?? 0;
              return `${context.dataset.label}: R$ ${value.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`;
            },
            footer: (items) => {
              if (oculto) return '';
              const i = items[0]?.dataIndex ?? 0;
              const info = this.valoresTooltip()[i];
              if (!info || info.previsto <= 0) return '';
              return `Inclui R$ ${info.previsto.toLocaleString('pt-BR', { minimumFractionDigits: 2 })} previstos`;
            }
          }
        }
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: { color: corTextoGrafico() }
        },
        y: {
          beginAtZero: true,
          grid: { color: corGradeGrafico() },
          border: { display: false },
          ticks: {
            color: corTextoGrafico(),
            callback: (value) => oculto ? '••••••' : `R$ ${Number(value).toLocaleString('pt-BR')}`
          }
        }
      }
    };
  });

  barChartMensalPlugins = computed(() => this.privacidade.valoresOcultos() ? [] : [criarRotuloValorBarras(0)]);
  barChartPorContaPlugins = computed(() => this.privacidade.valoresOcultos() ? [] : [criarRotuloValorBarras(2)]);

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

  doughnutChartData: ChartConfiguration<'doughnut'>['data'] = {
    labels: [],
    datasets: []
  };

  doughnutChartOptions = computed<ChartConfiguration<'doughnut'>['options']>(() => {
    const oculto = this.privacidade.valoresOcultos();
    return {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '68%',
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (context) => {
              if (oculto) return '••••••';
              const value = context.parsed ?? 0;
              return `${context.label}: R$ ${Number(value).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`;
            }
          }
        }
      }
    };
  });

  doughnutVazioOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: {
      legend: { display: false },
      tooltip: { enabled: false }
    }
  };

  donutVazioData: ChartConfiguration<'doughnut'>['data'] = {
    labels: ['Sem dados'],
    datasets: [{ data: [1], backgroundColor: [corGradeGrafico()], borderWidth: 0 }]
  };

  donutDistCatData: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [] };
  donutDistSubData: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [] };

  private requestSeq = 0;
  chartVersion = signal(0);
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;
  private graficoMensal = signal<{ label: string; mes: number; ano: number; d: Dashboard }[]>([]);
  valoresExibidos = signal<{ receitas: number; despesas: number; previsto: number }[]>([]);
  valoresTooltip = signal<{ previsto: number }[]>([]);
  modoPorConta = computed(() => (this.data()?.resumoPorConta?.length ?? 0) > 1);

  distMensalCategorias = computed<DistribuicaoCategoriaAnual[]>(() => {
    const d = this.data();
    if (!d) return [];
    return this.tipoDistribuicao() === 'receita' ? d.distribuicaoReceitas : d.distribuicaoDespesas;
  });

  distMensalSubcategorias = computed<DistribuicaoCategoriaAnual[]>(() =>
    this.achatarSubcategorias(this.distMensalCategorias()));

  totalDistCat = computed(() => this.distMensalCategorias().reduce((s, i) => s + i.total, 0));
  totalDistSub = computed(() => this.distMensalSubcategorias().reduce((s, i) => s + i.total, 0));
  filtroSerie = signal<'ambos' | 'receitas' | 'despesas'>('ambos');
  graficoColapsado = signal(false);

  rotuloMesAnterior = computed(() => {
    const m = this.mesAdjacente(this.mes(), this.ano(), -1);
    return `${m.mes < 10 ? '0' : ''}${m.mes}/${m.ano}`;
  });

  kpiMensal = computed(() => {
    const itens = this.graficoMensal();
    if (itens.length < 2) return null;
    const atual = itens[itens.length - 2];
    const anterior = itens[itens.length - 3];
    const v = this.valorExibido(atual.mes, atual.ano, atual.d);
    const va = this.valorExibido(anterior.mes, anterior.ano, anterior.d);
    const saldo = v.receitas - v.despesas;
    const saldoAnt = va.receitas - va.despesas;
    const dias = diasReferencia(atual.mes, atual.ano);
    const diasAnt = diasReferencia(anterior.mes, anterior.ano);
    const mediaDiaria = dias > 0 ? v.receitas / dias : 0;
    const mediaAnt = diasAnt > 0 ? va.receitas / diasAnt : 0;
    const fluxo = atual.d.totalRecebido - atual.d.totalPago;
    const fluxoAnt = anterior.d.totalRecebido - anterior.d.totalPago;
    const pagoParcerias = atual.d.resumoParcerias?.totalPago ?? 0;
    const pagoParceriasAnt = anterior.d.resumoParcerias?.totalPago ?? 0;
    return {
      receitas: v.receitas,
      despesas: v.despesas,
      saldo,
      mediaDiaria,
      dias,
      fluxo,
      varReceitas: calcularVariacao(v.receitas, va.receitas),
      varDespesas: calcularVariacao(v.despesas, va.despesas),
      varSaldo: calcularVariacao(saldo, saldoAnt),
      varMediaDiaria: calcularVariacao(mediaDiaria, mediaAnt),
      varFluxo: calcularVariacao(fluxo, fluxoAnt),
      varParcerias: calcularVariacao(pagoParcerias, pagoParceriasAnt),
      margem: v.receitas !== 0 ? Math.round(saldo / v.receitas * 1000) / 10 : null,
      despesaSobreReceita: v.receitas !== 0 ? Math.round(v.despesas / v.receitas * 1000) / 10 : null,
      pontoEquilibrio: v.despesas,
      distanciaPonto: v.despesas !== 0 ? Math.round(saldo / v.despesas * 1000) / 10 : null
    };
  });

  sparkOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false }, tooltip: { enabled: false } },
    scales: { x: { display: false }, y: { display: false } },
    elements: { point: { radius: 0 }, line: { tension: 0.4, borderWidth: 2 } }
  };

  sparkReceitas: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  sparkDespesas: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  sparkSaldo: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  sparkFluxo: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };

  private sparkVazio: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  private sparkExibicao(dados: ChartConfiguration<'line'>['data']) {
    this.chartVersion();
    return this.privacidade.valoresOcultos() ? this.sparkVazio : dados;
  }
  sparkReceitasExibicao = computed(() => this.sparkExibicao(this.sparkReceitas));
  sparkDespesasExibicao = computed(() => this.sparkExibicao(this.sparkDespesas));
  sparkSaldoExibicao = computed(() => this.sparkExibicao(this.sparkSaldo));
  sparkFluxoExibicao = computed(() => this.sparkExibicao(this.sparkFluxo));

  ngOnInit() { this.carregar(); this.carregarContas(); this.carregarContratos(); }

  async carregarContratos() {
    try {
      const contratos = await firstValueFrom(this.contratoRepo.listar(true));
      this.contratos.set(contratos ?? []);
    } catch { this.contratos.set([]); }
  }

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
        const idConta = this.filtroConta() || undefined;
        const meses = [-6, -5, -4, -3, -2, -1, 0, 1].map(d => this.mesAdjacente(mesAtual, anoAtual, d));
        const dashboards = await Promise.all(
          meses.map(m => firstValueFrom(this.repo.obter(m.mes, m.ano, idConta)))
        );
if (seq !== this.requestSeq) return;
            this.data.set(dashboards[6]);
            this.graficoMensal.set(meses.map((m, i) => ({ label: MESES[m.mes - 1], mes: m.mes, ano: m.ano, d: dashboards[i] })));
            this.montarSparks();
            this.atualizarDonutsDistribuicao();
            if ((this.data()?.resumoPorConta?.length ?? 0) > 1) this.atualizarGraficoPorConta();
            else this.atualizarGraficoMensal();
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
    this.valoresExibidos.set([]);
    this.valoresTooltip.set([]);
    this.barChartData = { labels: [], datasets: [] };
    this.doughnutChartData = { labels: [], datasets: [] };
    this.donutDistCatData = { labels: [], datasets: [] };
    this.donutDistSubData = { labels: [], datasets: [] };
    this.carregar();
  }

  onFiltroContaChange(valor: string) {
    this.filtroConta.set(valor);
    this.carregar();
  }

  trocarTipoDistribuicao(tipo: 'receita' | 'despesa') {
    this.tipoDistribuicao.set(tipo);
    this.atualizarDonut();
  }

  trocarNivelDistribuicao(nivel: 'categoria' | 'subcategoria') {
    this.nivelDistribuicao.set(nivel);
    this.atualizarDonut();
  }

  trocarTipoDistMensal(tipo: 'receita' | 'despesa') {
    this.tipoDistribuicao.set(tipo);
    this.atualizarDonutsDistribuicao();
  }

  private achatarSubcategorias(base: DistribuicaoCategoriaAnual[]): DistribuicaoCategoriaAnual[] {
    const flat: DistribuicaoCategoriaAnual[] = [];
    for (const cat of base) {
      for (const sub of cat.subcategorias ?? []) {
        flat.push({ nome: `${cat.nome} • ${sub.nome}`, total: sub.total, percentual: sub.percentual, subcategorias: [] });
      }
    }
    return flat.sort((a, b) => b.total - a.total);
  }

  private montarDonutDistribuicao(itens: DistribuicaoCategoriaAnual[]): ChartConfiguration<'doughnut'>['data'] {
    return {
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

  private atualizarDonutsDistribuicao() {
    this.donutDistCatData = this.montarDonutDistribuicao(this.distMensalCategorias());
    this.donutDistSubData = this.montarDonutDistribuicao(this.distMensalSubcategorias());
  }

  trocarFiltroSerie(filtro: 'ambos' | 'receitas' | 'despesas', event: MouseEvent) {
    event.stopPropagation();
    this.filtroSerie.set(filtro);
    this.aplicarFiltroSerie();
  }

  alternarGrafico() {
    this.graficoColapsado.update(v => !v);
  }

  private aplicarFiltroSerie() {
    const filtro = this.filtroSerie();
    this.barChartData = {
      ...this.barChartData,
      datasets: this.barChartData.datasets.map(ds => ({
        ...ds,
        hidden: (filtro === 'receitas' && ds.label !== 'Receitas') || (filtro === 'despesas' && ds.label !== 'Despesas')
      }))
    };
  }

  private montarSparks() {
    const itens = this.graficoMensal();
    if (itens.length < 2) return;
    const labels = itens.map(i => MESES[i.mes - 1].slice(0, 3));
    const rec = itens.map(i => this.valorExibido(i.mes, i.ano, i.d).receitas);
    const desp = itens.map(i => this.valorExibido(i.mes, i.ano, i.d).despesas);
    const saldos = rec.map((r, i) => r - desp[i]);
    const fluxos = itens.map(i => i.d.totalRecebido - i.d.totalPago);
    this.sparkReceitas = { labels, datasets: [{ data: rec, borderColor: corReceita(), fill: false }] };
    this.sparkDespesas = { labels, datasets: [{ data: desp, borderColor: corDespesa(), fill: false }] };
    this.sparkSaldo = { labels, datasets: [{ data: saldos, borderColor: corReceita(), fill: false }] };
    this.sparkFluxo = { labels, datasets: [{ data: fluxos, borderColor: corRoxa(), fill: false }] };
  }

  private atualizarGraficoMensal() {
    const todos = this.graficoMensal();
    if (todos.length < 4) return;
    const itens = todos.slice(-3);

    const valores = itens.map(i => this.valorExibido(i.mes, i.ano, i.d));
    this.valoresExibidos.set(valores);
    this.valoresTooltip.set(valores);

    this.barChartData = {
      labels: itens.map(i => i.label),
      datasets: [
        {
          data: valores.map(v => v.receitas),
          label: 'Receitas',
          backgroundColor: comTransparencia(corReceita()),
          borderWidth: 0,
          borderRadius: 6,
          borderSkipped: 'bottom',
          categoryPercentage: 0.55,
          barPercentage: 0.65
        },
        {
          data: valores.map(v => v.despesas),
          label: 'Despesas',
          backgroundColor: comTransparencia(corDespesa()),
          borderWidth: 0,
          borderRadius: 6,
          borderSkipped: 'bottom',
          categoryPercentage: 0.55,
          barPercentage: 0.65
        }
      ]
    };

    this.aplicarFiltroSerie();
  }

  private atualizarGraficoPorConta() {
    const mensal = this.data();
    if (!mensal?.resumoPorConta.length) return;

    const valores = mensal.resumoPorConta.map(c => this.valorExibidoConta(mensal.mes, mensal.ano, c));
    this.valoresTooltip.set(valores);

    this.barChartData = {
      labels: mensal.resumoPorConta.map(c => c.nomeConta),
      datasets: [
        {
          data: valores.map(v => v.receitas),
          label: 'Receitas',
          backgroundColor: comTransparencia(corReceita()),
          borderWidth: 0,
          borderRadius: 6,
          borderSkipped: 'bottom',
          categoryPercentage: 0.55,
          barPercentage: 0.65
        },
        {
          data: valores.map(v => v.despesas),
          label: 'Despesas',
          backgroundColor: comTransparencia(corDespesa()),
          borderWidth: 0,
          borderRadius: 6,
          borderSkipped: 'bottom',
          categoryPercentage: 0.55,
          barPercentage: 0.65
        }
      ]
    };
    this.aplicarFiltroSerie();
  }

  private valorExibidoConta(mes: number, ano: number, c: ResumoPorConta): { receitas: number; despesas: number; previsto: number } {
    const hoje = new Date();
    const atualOuFuturo = ano > hoje.getFullYear() || (ano === hoje.getFullYear() && mes >= hoje.getMonth() + 1);
    if (!atualOuFuturo) return { receitas: c.totalReceitas, despesas: c.totalDespesas, previsto: 0 };
    return {
      receitas: c.totalReceitas + c.totalReceitasPrevisto,
      despesas: c.totalDespesas + c.totalDespesasPrevisto,
      previsto: c.totalReceitasPrevisto + c.totalDespesasPrevisto
    };
  }

  private valorExibido(mes: number, ano: number, d: Dashboard): { receitas: number; despesas: number; previsto: number } {
    const hoje = new Date();
    const mesHoje = hoje.getMonth() + 1;
    const anoHoje = hoje.getFullYear();
    const atualOuFuturo = ano > anoHoje || (ano === anoHoje && mes >= mesHoje);
    if (!atualOuFuturo) return { receitas: d.totalReceitas, despesas: d.totalDespesas, previsto: 0 };
    return {
      receitas: d.totalReceitas + d.totalReceitasPrevisto,
      despesas: d.totalDespesas + d.totalDespesasPrevisto,
      previsto: d.totalReceitasPrevisto + d.totalDespesasPrevisto
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
          backgroundColor: comTransparencia(corReceita()),
          borderColor: corReceita(),
          borderWidth: 1
        },
        {
          data: anual.resumoPorMes.map(m => m.totalDespesas),
          label: 'Despesas',
          backgroundColor: comTransparencia(corDespesa()),
          borderColor: corDespesa(),
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
