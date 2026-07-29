import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DespesaRecorrenteRepository } from '../../core/repositories/recorrente.repository';
import { CategoriaDespesaRepository } from '../../core/repositories/categoria.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { DespesaMensalRepository } from '../../core/repositories/lancamento-mensal.repository';
import { Recorrente, RecorrenteRequest } from '../../core/models/recorrente.model';
import { LancamentoMensal } from '../../core/models/lancamento-mensal.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { SectionHeaderComponent } from '../../shared/components/section-header.component';
import { SkeletonComponent } from '../../shared/components/skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { TabsComponent, Tab } from '../../shared/components/tabs.component';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { RecorrenteModalComponent } from '../../shared/components/recorrente-modal.component';

@Component({
  selector: 'app-despesas',
  standalone: true,
  imports: [DatePipe, FormsModule, SectionHeaderComponent, SkeletonComponent, EmptyStateComponent, TabsComponent, MonthNavComponent, StatusBadgeComponent, RecorrenteModalComponent],
  template: `
    <div class="page">
      <app-section-header
        title="Despesas"
        subtitle="Gerencie suas despesas recorrentes e mensais"
        [showAdd]="tabAtiva() === 'recorrentes'"
        addLabel="Nova despesa"
        (add)="abrirModal()"
      />

      <app-tabs [tabs]="tabs" [active]="tabAtiva()" (change)="tabAtiva.set($event); onTabChange()" />

      @if (tabAtiva() === 'recorrentes') {
        @if (loading()) {
          <app-skeleton type="row" [count]="5" />
        } @else if (recorrentes().length === 0) {
          <app-empty-state title="Nenhuma despesa recorrente" description="Cadastre sua primeira despesa recorrente." actionLabel="Nova despesa" (action)="abrirModal()" />
        } @else {
          <div class="table-card">
            <table class="table">
              <thead><tr><th>Descrição</th><th>Valor</th><th>Dia</th><th>Categoria</th><th>Conta</th><th>Início</th><th>Fim</th><th></th></tr></thead>
              <tbody>
                @for (r of recorrentes(); track r.id) {
                  <tr>
                    <td class="cell-name">{{ r.descricao }}</td>
                    <td class="cell-value">R$ {{ r.valor.toFixed(2) }}</td>
                    <td>Dia {{ r.dia }}</td>
                    <td>{{ r.categoria }}</td>
                    <td>{{ r.conta }}</td>
                    <td class="cell-meta">{{ r.dataInicio | date:'MMM/yyyy' }}</td>
                    <td class="cell-meta">{{ r.dataFim ? (r.dataFim | date:'MMM/yyyy') : '—' }}</td>
                    <td class="cell-actions">
                      <button class="action-btn" title="Editar" (click)="abrirModal(r)">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/><path d="m15 5 4 4"/></svg>
                      </button>
                      <button class="action-btn action-btn--danger" title="Excluir" (click)="excluir(r)">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M3 6h18M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/></svg>
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }

      @if (tabAtiva() === 'mensal') {
        <div class="mensal-header">
          <app-month-nav [mes]="mes()" [ano]="ano()" (prev)="navegarMes(-1)" (next)="navegarMes(1)" />
          <div class="totals">
            <span class="total" style="font-size:1.25rem;font-weight:700">R$ {{ totalDespesas().toFixed(2) }}</span>
            <span class="realizado" style="color:var(--color-success);font-size:0.875rem">R$ {{ totalPago().toFixed(2) }} pago</span>
            <span class="perc" style="font-size:0.875rem;color:var(--text-muted)">{{ percPago() }}%</span>
          </div>
        </div>

        @if (loading()) {
          <app-skeleton type="row" [count]="5" />
        } @else if (mensais().length === 0) {
          <app-empty-state title="Nenhum lançamento neste mês" description="Cadastre uma despesa recorrente para gerar lançamentos." actionLabel="Ir para Recorrentes" (action)="tabAtiva.set('recorrentes')" />
        } @else {
          <div class="table-card">
            <table class="table">
              <thead><tr><th>Descrição</th><th>Valor</th><th>Status</th><th>Data</th><th></th></tr></thead>
              <tbody>
                @for (l of mensais(); track l.id) {
                  <tr>
                    <td class="cell-name">{{ l.descricao }}</td>
                    <td class="cell-value">R$ {{ l.valor.toFixed(2) }}</td>
                    <td><app-status-badge [type]="l.status === 'Realizado' ? 'realizado' : 'pendente'" [label]="l.status === 'Realizado' ? 'Pago' : 'Pendente'" /></td>
                    <td class="cell-meta">{{ l.dataRealizacao ? (l.dataRealizacao | date:'dd/MM/yyyy') : '—' }}</td>
                    <td class="cell-actions">
                      @if (l.status !== 'Realizado') {
                        <button class="action-btn action-btn--success" title="Pagar" (click)="pagar(l)">
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><polyline points="20 6 9 17 4 12"/></svg>
                        </button>
                      } @else {
                        <button class="action-btn" title="Estornar" (click)="estornar(l)">
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><polyline points="1 4 1 10 7 10"/><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10"/></svg>
                        </button>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }
    </div>

    <app-recorrente-modal
      [visible]="modalVisible()"
      [editando]="editando()"
      tipoLabel="despesa"
      [categorias]="categorias()"
      [contas]="contas()"
      [salvando]="salvando()"
      (visibleChange)="fecharModal()"
      (saved)="salvar($event)"
    />
  `,
  styles: [`
    .page { max-width: 1100px; }
    .table-card { background: var(--content-surface); border: 1px solid var(--surface-border); border-radius: var(--radius-lg); overflow: hidden; }
    .table { width: 100%; border-collapse: collapse; }
    .table th { text-align: left; padding: 0.75rem 1rem; font-size: 0.75rem; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em; border-bottom: 1px solid var(--surface-border); }
    .table td { padding: 0.75rem 1rem; font-size: 0.875rem; color: var(--text-primary); border-bottom: 1px solid var(--surface-border); }
    .table tr:last-child td { border-bottom: none; }
    .table tr:hover { background: var(--surface-hover); }
    .cell-name { font-weight: 500; white-space: nowrap; }
    .cell-value { font-variant-numeric: tabular-nums; white-space: nowrap; }
    .cell-meta { color: var(--text-muted); font-size: 0.8125rem; }
    .cell-actions { display: flex; gap: 0.25rem; justify-content: flex-end; }
    .action-btn { background: none; border: 1px solid var(--surface-border); border-radius: var(--radius-md); padding: 0.375rem; display: flex; color: var(--text-muted); cursor: pointer; transition: all var(--transition-fast); }
    .action-btn:hover { border-color: var(--color-primary); color: var(--color-primary); }
    .action-btn--danger:hover { border-color: var(--color-error); color: var(--color-error); }
    .action-btn--success:hover { border-color: var(--color-success); color: var(--color-success); }
    .mensal-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; flex-wrap: wrap; gap: 1rem; }
    .totals { display: flex; align-items: center; gap: 1rem; }
  `]
})
export class DespesasComponent implements OnInit {
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private repo = inject(DespesaRecorrenteRepository);
  private repoMensal = inject(DespesaMensalRepository);
  private catRepo = inject(CategoriaDespesaRepository);
  private contaRepo = inject(ContaBancariaRepository);

  tabs: Tab[] = [
    { id: 'recorrentes', label: 'Recorrentes' },
    { id: 'mensal', label: 'Mensal' }
  ];
  tabAtiva = signal('recorrentes');

  recorrentes = signal<Recorrente[]>([]);
  mensais = signal<LancamentoMensal[]>([]);
  categorias = signal<any[]>([]);
  contas = signal<any[]>([]);

  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Recorrente | null>(null);
  salvando = signal(false);

  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());

  async ngOnInit() {
    await Promise.all([
      this.carregarRecorrentes(),
      this.carregarCategorias(),
      this.carregarContas()
    ]);
  }

  onTabChange() {
    if (this.tabAtiva() === 'mensal') this.carregarMensal();
  }

  async carregarRecorrentes() {
    this.loading.set(true);
    try { this.recorrentes.set(await firstValueFrom(this.repo.listar(this.auth.user()!.usuarioId))); }
    catch { this.notify.error('Erro ao carregar despesas'); }
    finally { this.loading.set(false); }
  }

  async carregarCategorias() {
    try { this.categorias.set(await firstValueFrom(this.catRepo.listar(this.auth.user()!.usuarioId))); } catch {}
  }

  async carregarContas() {
    try { this.contas.set(await firstValueFrom(this.contaRepo.listar(this.auth.user()!.usuarioId))); } catch {}
  }

  async carregarMensal() {
    this.loading.set(true);
    try { this.mensais.set(await firstValueFrom(this.repoMensal.listarPorMes(this.auth.user()!.usuarioId, this.mes(), this.ano()))); }
    catch { this.notify.error('Erro ao carregar lançamentos'); }
    finally { this.loading.set(false); }
  }

  abrirModal(item?: Recorrente) { this.editando.set(item ?? null); this.modalVisible.set(true); }
  fecharModal() { this.modalVisible.set(false); this.editando.set(null); }

  async salvar(data: RecorrenteRequest) {
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, data));
        this.notify.success('Despesa atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.auth.user()!.usuarioId, data));
        this.notify.success('Despesa criada');
      }
      this.fecharModal();
      await this.carregarRecorrentes();
    } catch { this.notify.error('Erro ao salvar despesa'); }
    finally { this.salvando.set(false); }
  }

  async excluir(item: Recorrente) {
    const ok = await this.confirmService.confirm('Excluir despesa', `Deseja excluir "${item.descricao}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Despesa excluída');
      await this.carregarRecorrentes();
    } catch { this.notify.error('Erro ao excluir despesa'); }
  }

  async pagar(lancamento: LancamentoMensal) {
    try {
      await firstValueFrom(this.repoMensal.pagar(lancamento.id, { data: new Date().toISOString().split('T')[0] }));
      this.notify.success('Despesa paga');
      await this.carregarMensal();
    } catch { this.notify.error('Erro ao pagar'); }
  }

  async estornar(lancamento: LancamentoMensal) {
    try {
      await firstValueFrom(this.repoMensal.estornar(lancamento.id));
      this.notify.success('Despesa estornada');
      await this.carregarMensal();
    } catch { this.notify.error('Erro ao estornar'); }
  }

  navegarMes(dir: number) {
    let m = this.mes() + dir, a = this.ano();
    if (m > 12) { m = 1; a++; }
    if (m < 1) { m = 12; a--; }
    this.mes.set(m); this.ano.set(a);
    this.carregarMensal();
  }

  totalDespesas() { return this.mensais().reduce((s, l) => s + l.valor, 0); }
  totalPago() { return this.mensais().filter(l => l.status === 'Realizado').reduce((s, l) => s + l.valor, 0); }
  percPago() { const t = this.totalDespesas(); return t ? Math.round(this.totalPago() / t * 100) : 0; }
}
