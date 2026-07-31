import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ReceitaRepository, ReceitaFiltros } from '../../core/repositories/receita.repository';
import { CategoriaReceitaRepository } from '../../core/repositories/categoria.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { Receita, ReceitaRequest } from '../../core/models/receita.model';
import { Categoria } from '../../core/models/categoria.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { LancamentoModalComponent, LancamentoForm } from '../../shared/components/lancamento-modal.component';
import { CurrencyBRLPipe } from '../../shared/pipes/currency-brl.pipe';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-receitas',
  standalone: true,
  imports: [DatePipe, FormsModule, MonthNavComponent, StatusBadgeComponent, LancamentoModalComponent, CurrencyBRLPipe, CustomSelectComponent, ListPaginationComponent, LucideDynamicIcon],
  template: `
    <div class="page">
      <header class="page__header">
        <div class="page__header-left">
          <svg lucideIcon="trending-up" class="page__icon" [size]="22" />
          <div>
            <h1 class="page__title">Receitas</h1>
            <p class="page__subtitle">Receitas do mês — recorrentes e avulsas</p>
          </div>
        </div>
        <button class="add-btn" (click)="abrirModal()">
          <svg lucideIcon="plus" [size]="16" />
          Nova receita
        </button>
      </header>

      <div class="toolbar">
        <app-month-nav [mes]="mes()" [ano]="ano()" (prev)="navegarMes(-1)" (next)="navegarMes(1)" />
        <div class="filtros">
          <app-custom-select placeholder="Todas as contas" [options]="contasOptions()" (valueChange)="filtroConta = $event; carregar()" />
          <app-custom-select placeholder="Todos os status" [options]="statusOptions" (valueChange)="filtroStatus = $event; carregar()" />
          <app-custom-select placeholder="Todas as categorias" [options]="categoriasOptions()" (valueChange)="filtroCategoria = $event; carregar()" />
          <div class="search-wrapper">
            <svg lucideIcon="search" [size]="16" class="search-icon" />
            <input [(ngModel)]="busca" (ngModelChange)="onBuscaChange()" placeholder="Buscar..." class="search-input" />
          </div>
        </div>
      </div>

      <div class="totals">
        <span>Total: <strong>{{ total() | currencyBRL }}</strong></span>
        <span>Recebido: <strong style="color: var(--color-success)">{{ totalRecebido() | currencyBRL }}</strong></span>
        <span>Pendente: <strong style="color: var(--color-warning)">{{ totalPendente() | currencyBRL }}</strong></span>
      </div>

      @if (loading()) {
        <div class="table-card">
          @for (i of [1,2,3,4,5]; track i) {
            <div class="skeleton-row">
              <div class="skeleton-line" style="width: 40%"></div>
              <div class="skeleton-line" style="width: 15%"></div>
              <div class="skeleton-line" style="width: 10%"></div>
            </div>
          }
        </div>
      } @else if (items().length === 0) {
        <div class="empty-state">
          <svg lucideIcon="inbox" [size]="48" class="empty-icon" />
          <h3>Nenhuma receita neste mês</h3>
          <p>Cadastre sua primeira receita — recorrente ou avulsa.</p>
          <button class="add-btn" (click)="abrirModal()">
            <svg lucideIcon="plus" [size]="16" />
            Nova receita
          </button>
        </div>
      } @else {
        <div class="table-card">
          <table class="table">
            <thead><tr><th>Descrição</th><th>Valor</th><th>Data</th><th>Conta</th><th>Categoria</th><th>Status</th><th></th></tr></thead>
            <tbody>
              @for (l of pagination.paginatedItems(); track l.id) {
                <tr>
                  <td class="cell-name" data-label="Descrição">{{ l.descricao }}</td>
                  <td class="cell-value" data-label="Valor">{{ l.valor | currencyBRL }}</td>
                  <td class="cell-meta" data-label="Data">{{ l.data | date:'dd/MM' }}</td>
                  <td class="cell-meta" data-label="Conta">{{ l.conta }}</td>
                  <td class="cell-meta" data-label="Categoria">{{ l.categoria }}{{ l.subcategoria ? ' → ' + l.subcategoria : '' }}</td>
                  <td data-label="Status"><app-status-badge [type]="l.status === 'Realizado' ? 'realizado' : 'pendente'" [label]="l.status === 'Realizado' ? 'Recebido' : 'Pendente'" /></td>
                  <td class="cell-actions">
                    @if (l.status !== 'Realizado') {
                      <button class="action-btn action-btn--success" title="Receber" (click)="receber(l)">
                        <svg lucideIcon="check" [size]="16" />
                      </button>
                    } @else {
                      <button class="action-btn" title="Estornar" (click)="estornar(l)">
                        <svg lucideIcon="arrow-left" [size]="16" />
                      </button>
                    }
                    <button class="action-btn" title="Editar" (click)="abrirModal(l)">
                      <svg lucideIcon="pencil" [size]="16" />
                    </button>
                    <button class="action-btn action-btn--danger" title="Excluir" (click)="excluir(l)">
                      <svg lucideIcon="trash-2" [size]="16" />
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
          <app-list-pagination [pagination]="pagination" />
        </div>
      }
    </div>

    <app-lancamento-modal
      [visible]="modalVisible()"
      [editando]="editando()"
      tipoLabel="receita"
      [categorias]="categorias()"
      [contas]="contas()"
      [salvando]="salvando()"
      (visibleChange)="fecharModal()"
      (saved)="salvar($event)"
    />
  `,
  styles: [`
    .page { max-width: 1200px; padding: 1.5rem; margin: 0 auto; }
    .page__header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1.5rem; gap: 1rem; }
    .page__header-left { display: flex; align-items: center; gap: 0.5rem; }
    .page__icon { color: var(--color-primary); flex-shrink: 0; }
    .page__title { font-size: 1.5rem; font-weight: 600; color: var(--text-primary); margin: 0; }
    .page__subtitle { font-size: 0.875rem; color: var(--text-muted); margin-top: 0.25rem; }
    .add-btn {
      display: flex; align-items: center; gap: 0.375rem; padding: 0.5rem 1rem;
      background: var(--color-primary); color: #fff; border: none; border-radius: var(--radius-md);
      font-size: 0.875rem; font-weight: 500; white-space: nowrap;
      transition: background var(--transition-fast);
    }
    .add-btn:hover { background: var(--color-primary-hover); }
    .toolbar {
      display: flex; align-items: center; justify-content: space-between; gap: 1rem;
      margin-bottom: 1rem; flex-wrap: wrap;
    }
    .filtros { display: flex; gap: 0.5rem; flex-wrap: wrap; align-items: flex-end; }
    .search-wrapper { position: relative; }
    .search-icon { position: absolute; left: 0.75rem; top: 50%; transform: translateY(-50%); color: var(--text-muted); }
    .search-input {
      padding: 0.625rem 0.75rem 0.625rem 2.25rem;
      border: 1px solid var(--surface-border); border-radius: var(--radius-md);
      font-size: 0.875rem; color: var(--text-primary); background: var(--content-surface);
      min-width: 180px; transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    }
    .search-input:focus { outline: none; border-color: var(--color-primary); box-shadow: 0 0 0 3px var(--color-primary-focus-ring); }
    .search-input::placeholder { color: var(--text-muted); }
    .totals { display: flex; gap: 1.5rem; margin-bottom: 1rem; font-size: 0.875rem; color: var(--text-secondary); flex-wrap: wrap; }
    .table-card {
      background: var(--content-surface); border: 1px solid var(--surface-border);
      border-radius: var(--radius-xl); overflow: hidden;
    }
    .table { width: 100%; border-collapse: collapse; }
    .table th { text-align: left; padding: 0.75rem 1rem; font-size: 0.75rem; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em; border-bottom: 1px solid var(--surface-border); }
    .table td { padding: 0.75rem 1rem; font-size: 0.875rem; color: var(--text-primary); border-bottom: 1px solid var(--surface-border); }
    .table tr:last-child td { border-bottom: none; }
    .table tr:hover { background: var(--surface-hover); }
    .cell-name { font-weight: 500; }
    .cell-value { font-variant-numeric: tabular-nums; white-space: nowrap; }
    .cell-meta { color: var(--text-muted); font-size: 0.8125rem; }
    .cell-actions { display: flex; gap: 0.25rem; justify-content: flex-end; }
    .action-btn {
      background: none; border: 1px solid var(--surface-border); border-radius: var(--radius-md);
      padding: 0.375rem; display: flex; color: var(--text-muted); cursor: pointer;
      transition: all var(--transition-fast);
    }
    .action-btn:hover { border-color: var(--color-primary); color: var(--color-primary); }
    .action-btn--danger:hover { border-color: var(--color-error); color: var(--color-error); }
    .action-btn--success:hover { border-color: var(--color-success); color: var(--color-success); }
    .empty-state {
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      padding: 3rem 1rem; text-align: center;
    }
    .empty-icon { color: var(--text-muted); margin-bottom: 1rem; }
    .empty-state h3 { font-size: 1rem; font-weight: 600; color: var(--text-primary); margin-bottom: 0.5rem; }
    .empty-state p { font-size: 0.875rem; color: var(--text-muted); margin-bottom: 1.25rem; }
    .skeleton-row { display: flex; gap: 1rem; padding: 0.75rem 1rem; border-bottom: 1px solid var(--surface-border); }
    .skeleton-line { height: 1rem; background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%); background-size: 200% 100%; animation: shimmer 1.5s infinite; border-radius: var(--radius-sm); }
    @keyframes shimmer { 0% { background-position: -200% 0; } 100% { background-position: 200% 0; } }
    @media (max-width: 767px) {
      .page { padding: 0.75rem; }
      .page__header { flex-direction: column; align-items: stretch; }
      .page__title { font-size: 1.25rem; }
      .toolbar { flex-direction: column; align-items: stretch; }
      .filtros { flex-direction: column; }
      .search-input { min-width: 0; width: 100%; }
      .table th { display: none; }
      .table td { display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 1rem; }
      .table td::before { content: attr(data-label); font-weight: 600; font-size: 0.75rem; color: var(--text-muted); }
      .table tr { display: block; border: 1px solid var(--surface-border); border-radius: var(--radius-lg); margin-bottom: 0.5rem; }
      .table tr:hover { background: transparent; }
    }
  `]
})
export class ReceitasComponent implements OnInit {
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private repo = inject(ReceitaRepository);
  private catRepo = inject(CategoriaReceitaRepository);
  private contaRepo = inject(ContaBancariaRepository);

  items = signal<Receita[]>([]);
  categorias = signal<Categoria[]>([]);
  contas = signal<ContaBancaria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Receita | null>(null);
  salvando = signal(false);

  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());
  filtroConta = '';
  filtroStatus = '';
  filtroCategoria = '';
  busca = '';

  statusOptions: SelectOption[] = [
    { value: 'Pendente', label: 'Pendentes' },
    { value: 'Realizado', label: 'Recebidas' },
  ];

  contasOptions = computed(() => this.contas().map(c => ({ value: c.id, label: `${c.nome} (${c.banco})` })));
  categoriasOptions = computed(() => this.categorias().map(c => ({ value: c.id, label: c.nome })));

  pagination = useListPagination(this.items, { initialPageSize: 10 });

  async ngOnInit() {
    await Promise.all([this.carregarCategorias(), this.carregarContas()]);
    await this.carregar();
  }

  async carregar() {
    this.loading.set(true);
    try {
      const filtros: ReceitaFiltros = {
        idUsuario: this.auth.user()!.usuarioId,
        mes: this.mes(),
        ano: this.ano(),
        idConta: this.filtroConta || undefined,
        status: this.filtroStatus || undefined,
        idCategoria: this.filtroCategoria || undefined,
        busca: this.busca || undefined
      };
      this.items.set(await firstValueFrom(this.repo.listar(filtros)));
    } catch { this.notify.error('Erro ao carregar receitas'); }
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

  abrirModal(item?: Receita) { this.editando.set(item ?? null); this.modalVisible.set(true); }
  fecharModal() { this.modalVisible.set(false); this.editando.set(null); }

  async salvar(data: LancamentoForm) {
    this.salvando.set(true);
    try {
      const request: ReceitaRequest = {
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
        this.notify.success('Receita atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.auth.user()!.usuarioId, request));
        this.notify.success(data.repete ? 'Receita recorrente criada' : 'Receita criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch { this.notify.error('Erro ao salvar receita'); }
    finally { this.salvando.set(false); }
  }

  async receber(item: Receita) {
    try {
      await firstValueFrom(this.repo.receber(item.id, { data: new Date().toISOString().split('T')[0] }));
      this.notify.success('Receita recebida');
      await this.carregar();
    } catch { this.notify.error('Erro ao receber'); }
  }

  async estornar(item: Receita) {
    try {
      await firstValueFrom(this.repo.estornar(item.id));
      this.notify.success('Receita estornada');
      await this.carregar();
    } catch { this.notify.error('Erro ao estornar'); }
  }

  async excluir(item: Receita) {
    const ok = await this.confirmService.confirm('Excluir receita', `Deseja excluir "${item.descricao}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Receita excluída');
      await this.carregar();
    } catch { this.notify.error('Erro ao excluir receita'); }
  }

  total() { return this.items().reduce((s, l) => s + l.valor, 0); }
  totalRecebido() { return this.items().filter(l => l.status === 'Realizado').reduce((s, l) => s + l.valor, 0); }
  totalPendente() { return this.total() - this.totalRecebido(); }
}
