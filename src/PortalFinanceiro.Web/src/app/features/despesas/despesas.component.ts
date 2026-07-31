import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DespesaRepository, DespesaFiltros } from '../../core/repositories/despesa.repository';
import { CategoriaDespesaRepository } from '../../core/repositories/categoria.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { Despesa, DespesaRequest } from '../../core/models/despesa.model';
import { Categoria } from '../../core/models/categoria.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { SectionHeaderComponent } from '../../shared/components/section-header.component';
import { SkeletonComponent } from '../../shared/components/skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { LancamentoModalComponent, LancamentoForm } from '../../shared/components/lancamento-modal.component';
import { CurrencyBRLPipe } from '../../shared/pipes/currency-brl.pipe';

@Component({
  selector: 'app-despesas',
  standalone: true,
  imports: [DatePipe, FormsModule, SectionHeaderComponent, SkeletonComponent, EmptyStateComponent, MonthNavComponent, StatusBadgeComponent, LancamentoModalComponent, CurrencyBRLPipe],
  template: `
    <div class="page">
      <app-section-header
        title="Despesas"
        subtitle="Despesas do mês — recorrentes e avulsas"
        addLabel="Nova despesa"
        (add)="abrirModal()"
      />

      <div class="toolbar">
        <app-month-nav [mes]="mes()" [ano]="ano()" (prev)="navegarMes(-1)" (next)="navegarMes(1)" />
        <div class="filtros">
          <select [(ngModel)]="filtroConta" (ngModelChange)="carregar()" class="select filtro">
            <option value="">Todas as contas</option>
            @for (c of contas(); track c.id) {
              <option [value]="c.id">{{ c.nome }}</option>
            }
          </select>
          <select [(ngModel)]="filtroStatus" (ngModelChange)="carregar()" class="select filtro">
            <option value="">Todos os status</option>
            <option value="Pendente">Pendentes</option>
            <option value="Realizado">Pagas</option>
          </select>
          <select [(ngModel)]="filtroCategoria" (ngModelChange)="carregar()" class="select filtro">
            <option value="">Todas as categorias</option>
            @for (c of categorias(); track c.id) {
              <option [value]="c.id">{{ c.nome }}</option>
            }
          </select>
          <input [(ngModel)]="busca" (ngModelChange)="onBuscaChange()" placeholder="Buscar..." class="input filtro-busca" />
        </div>
      </div>

      <div class="totals">
        <span>Total: <strong>{{ total() | currencyBRL }}</strong></span>
        <span>Pago: <strong style="color: var(--color-success)">{{ totalPago() | currencyBRL }}</strong></span>
        <span>Pendente: <strong style="color: var(--color-warning)">{{ totalPendente() | currencyBRL }}</strong></span>
      </div>

      @if (loading()) {
        <app-skeleton type="row" [count]="5" />
      } @else if (items().length === 0) {
        <app-empty-state title="Nenhuma despesa neste mês" description="Cadastre sua primeira despesa — recorrente ou avulsa." actionLabel="Nova despesa" (action)="abrirModal()" />
      } @else {
        <div class="table-card">
          <table class="table">
            <thead><tr><th>Descrição</th><th>Valor</th><th>Data</th><th>Conta</th><th>Categoria</th><th>Status</th><th></th></tr></thead>
            <tbody>
              @for (l of items(); track l.id) {
                <tr>
                  <td class="cell-name">{{ l.descricao }}</td>
                  <td class="cell-value">{{ l.valor | currencyBRL }}</td>
                  <td class="cell-meta">{{ l.data | date:'dd/MM' }}</td>
                  <td class="cell-meta">{{ l.conta }}</td>
                  <td class="cell-meta">{{ l.categoria }}{{ l.subcategoria ? ' → ' + l.subcategoria : '' }}</td>
                  <td><app-status-badge [type]="l.status === 'Realizado' ? 'realizado' : 'pendente'" [label]="l.status === 'Realizado' ? 'Pago' : 'Pendente'" /></td>
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
                    <button class="action-btn" title="Editar" (click)="abrirModal(l)">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/><path d="m15 5 4 4"/></svg>
                    </button>
                    <button class="action-btn action-btn--danger" title="Excluir" (click)="excluir(l)">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M3 6h18M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/></svg>
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>

    <app-lancamento-modal
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
    .page { max-width: 1200px; }
    .toolbar { display: flex; align-items: center; justify-content: space-between; gap: 1rem; margin-bottom: 1rem; flex-wrap: wrap; }
    .filtros { display: flex; gap: 0.5rem; flex-wrap: wrap; }
    .filtro { min-width: 140px; }
    .filtro-busca { min-width: 160px; }
    .select, .input { padding: 0.5rem 0.75rem; border: 1px solid var(--surface-border); border-radius: var(--radius-md); font-size: 0.875rem; color: var(--text-primary); background: var(--content-surface); }
    .select { appearance: none; background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 24 24' fill='none' stroke='%2394a3b8' stroke-width='2' stroke-linecap='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right 0.75rem center; padding-right: 2.25rem; cursor: pointer; }
    .totals { display: flex; gap: 1.5rem; margin-bottom: 1rem; font-size: 0.875rem; color: var(--text-secondary); flex-wrap: wrap; }
    .table-card { background: var(--content-surface); border: 1px solid var(--surface-border); border-radius: var(--radius-lg); overflow: hidden; }
    .table { width: 100%; border-collapse: collapse; }
    .table th { text-align: left; padding: 0.75rem 1rem; font-size: 0.75rem; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em; border-bottom: 1px solid var(--surface-border); }
    .table td { padding: 0.75rem 1rem; font-size: 0.875rem; color: var(--text-primary); border-bottom: 1px solid var(--surface-border); }
    .table tr:last-child td { border-bottom: none; }
    .table tr:hover { background: var(--surface-hover); }
    .cell-name { font-weight: 500; }
    .cell-value { font-variant-numeric: tabular-nums; white-space: nowrap; }
    .cell-meta { color: var(--text-muted); font-size: 0.8125rem; }
    .cell-actions { display: flex; gap: 0.25rem; justify-content: flex-end; }
    .action-btn { background: none; border: 1px solid var(--surface-border); border-radius: var(--radius-md); padding: 0.375rem; display: flex; color: var(--text-muted); cursor: pointer; transition: all var(--transition-fast); }
    .action-btn:hover { border-color: var(--color-primary); color: var(--color-primary); }
    .action-btn--danger:hover { border-color: var(--color-error); color: var(--color-error); }
    .action-btn--success:hover { border-color: var(--color-success); color: var(--color-success); }
  `]
})
export class DespesasComponent implements OnInit {
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private repo = inject(DespesaRepository);
  private catRepo = inject(CategoriaDespesaRepository);
  private contaRepo = inject(ContaBancariaRepository);

  items = signal<Despesa[]>([]);
  categorias = signal<Categoria[]>([]);
  contas = signal<ContaBancaria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Despesa | null>(null);
  salvando = signal(false);

  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());
  filtroConta = '';
  filtroStatus = '';
  filtroCategoria = '';
  busca = '';

  async ngOnInit() {
    await Promise.all([this.carregarCategorias(), this.carregarContas()]);
    await this.carregar();
  }

  async carregar() {
    this.loading.set(true);
    try {
      const filtros: DespesaFiltros = {
        idUsuario: this.auth.user()!.usuarioId,
        mes: this.mes(),
        ano: this.ano(),
        idConta: this.filtroConta || undefined,
        status: this.filtroStatus || undefined,
        idCategoria: this.filtroCategoria || undefined,
        busca: this.busca || undefined
      };
      this.items.set(await firstValueFrom(this.repo.listar(filtros)));
    } catch { this.notify.error('Erro ao carregar despesas'); }
    finally { this.loading.set(false); }
  }

  async carregarCategorias() {
    try { this.categorias.set(await firstValueFrom(this.catRepo.listar(this.auth.user()!.usuarioId))); } catch {}
  }

  async carregarContas() {
    try { this.contas.set(await firstValueFrom(this.contaRepo.listar(this.auth.user()!.usuarioId))); } catch {}
  }

  onBuscaChange() {
    clearTimeout((this as any)._buscaTimer);
    (this as any)._buscaTimer = setTimeout(() => this.carregar(), 400);
  }

  navegarMes(dir: number) {
    let m = this.mes() + dir, a = this.ano();
    if (m > 12) { m = 1; a++; }
    if (m < 1) { m = 12; a--; }
    this.mes.set(m); this.ano.set(a);
    this.carregar();
  }

  abrirModal(item?: Despesa) { this.editando.set(item ?? null); this.modalVisible.set(true); }
  fecharModal() { this.modalVisible.set(false); this.editando.set(null); }

  async salvar(data: LancamentoForm) {
    this.salvando.set(true);
    try {
      const request: DespesaRequest = {
        descricao: data.descricao,
        valor: data.valor,
        data: data.data + 'T00:00:00',
        idConta: data.idConta,
        idCategoria: data.idCategoria,
        idSubcategoria: data.idSubcategoria || undefined,
        repete: data.repete,
        dia: data.repete ? data.dia : undefined,
        diaUtil: data.repete ? data.diaUtil : undefined,
        dataFim: data.repete ? data.dataFim + 'T00:00:00' : undefined
      };
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, request));
        this.notify.success('Despesa atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.auth.user()!.usuarioId, request));
        this.notify.success(data.repete ? 'Despesa recorrente criada' : 'Despesa criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch { this.notify.error('Erro ao salvar despesa'); }
    finally { this.salvando.set(false); }
  }

  async pagar(item: Despesa) {
    try {
      await firstValueFrom(this.repo.pagar(item.id, { data: new Date().toISOString().split('T')[0] }));
      this.notify.success('Despesa paga');
      await this.carregar();
    } catch { this.notify.error('Erro ao pagar'); }
  }

  async estornar(item: Despesa) {
    try {
      await firstValueFrom(this.repo.estornar(item.id));
      this.notify.success('Despesa estornada');
      await this.carregar();
    } catch { this.notify.error('Erro ao estornar'); }
  }

  async excluir(item: Despesa) {
    const ok = await this.confirmService.confirm('Excluir despesa', `Deseja excluir "${item.descricao}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Despesa excluída');
      await this.carregar();
    } catch { this.notify.error('Erro ao excluir despesa'); }
  }

  total() { return this.items().reduce((s, l) => s + l.valor, 0); }
  totalPago() { return this.items().filter(l => l.status === 'Realizado').reduce((s, l) => s + l.valor, 0); }
  totalPendente() { return this.total() - this.totalPago(); }
}
