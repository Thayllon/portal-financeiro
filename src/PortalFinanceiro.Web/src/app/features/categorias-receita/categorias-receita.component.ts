import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CategoriaReceitaRepository, CategoriaDespesaRepository } from '../../core/repositories/categoria.repository';
import { Categoria, CategoriaRequest } from '../../core/models/categoria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { TabsComponent, Tab } from '../../shared/components/tabs.component';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-categorias',
  standalone: true,
  imports: [FormsModule, ModalComponent, TabsComponent, LucideDynamicIcon],
  template: `
    <div class="page">
      <header class="page__header">
        <div class="page__header-left">
          <svg lucideIcon="tag" class="page__icon" [size]="22" />
          <div>
            <h1 class="page__title">Categorias</h1>
            <p class="page__subtitle">Gerencie categorias e subcategorias de receitas e despesas</p>
          </div>
        </div>
        <button class="add-btn" (click)="abrirModal()">
          <svg lucideIcon="plus" [size]="16" />
          Nova categoria
        </button>
      </header>

      <app-tabs [tabs]="tabs" [active]="tabAtiva()" (change)="trocarAba($event)" />

      @if (loading()) {
        <div class="table-card">
          @for (i of [1,2,3,4]; track i) {
            <div class="skeleton-row">
              <div class="skeleton-line" style="width: 40%"></div>
              <div class="skeleton-line" style="width: 15%"></div>
            </div>
          }
        </div>
      } @else if (items().length === 0) {
        <div class="empty-state">
          <svg lucideIcon="tag" [size]="48" class="empty-icon" />
          <h3>Nenhuma categoria</h3>
          <p>Cadastre sua primeira categoria {{ tabAtiva() }}.</p>
          <button class="add-btn" (click)="abrirModal()">
            <svg lucideIcon="plus" [size]="16" />
            Nova categoria
          </button>
        </div>
      } @else {
        <div class="table-card">
          <table class="table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Tipo</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (c of categoriasPai(); track c.id) {
                <tr class="categoria-pai">
                  <td class="cell-name" data-label="Nome"><strong>{{ c.nome }}</strong></td>
                  <td data-label="Tipo"><span class="badge-cat">Categoria</span></td>
                  <td class="cell-actions">
                    <button class="action-btn" title="Nova subcategoria" (click)="abrirModal(undefined, c.id)">
                      <svg lucideIcon="plus" [size]="16" />
                    </button>
                    <button class="action-btn" title="Editar" (click)="abrirModal(c)">
                      <svg lucideIcon="pencil" [size]="16" />
                    </button>
                    <button class="action-btn action-btn--danger" title="Excluir" (click)="excluir(c)">
                      <svg lucideIcon="trash-2" [size]="16" />
                    </button>
                  </td>
                </tr>
                @for (sub of subcategoriasDe(c.id); track sub.id) {
                  <tr class="subcategoria">
                    <td class="cell-name cell-sub" data-label="Nome">↳ {{ sub.nome }}</td>
                    <td data-label="Tipo"><span class="badge-sub">Subcategoria</span></td>
                    <td class="cell-actions">
                      <button class="action-btn" title="Editar" (click)="abrirModal(sub)">
                        <svg lucideIcon="pencil" [size]="16" />
                      </button>
                      <button class="action-btn action-btn--danger" title="Excluir" (click)="excluir(sub)">
                        <svg lucideIcon="trash-2" [size]="16" />
                      </button>
                    </td>
                  </tr>
                }
              }
            </tbody>
          </table>
        </div>
      }
    </div>

    <app-modal
      [visible]="modalVisible()"
      [title]="'Nova categoria de ' + tabAtiva()"
      [saving]="salvando()"
      (visibleChange)="fecharModal()"
      (save)="salvar()"
    >
      <div class="field">
        <label>Nome</label>
        <input [(ngModel)]="form.nome" placeholder="Ex: Lazer" class="input" />
      </div>
      @if (!form.categoriaPaiId) {
        <div class="field">
          <label>Categoria pai (opcional)</label>
          <select [(ngModel)]="form.categoriaPaiId" class="select">
            <option value="">— Nenhuma (categoria principal)</option>
            @for (c of categoriasPai(); track c.id) {
              <option [value]="c.id">{{ c.nome }}</option>
            }
          </select>
        </div>
      }
    </app-modal>
  `,
  styles: [`
    .page { max-width: 900px; padding: 1.5rem; margin: 0 auto; }
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
    .table-card {
      background: var(--content-surface); border: 1px solid var(--surface-border);
      border-radius: var(--radius-xl); overflow: hidden;
    }
    .table { width: 100%; border-collapse: collapse; }
    .table th { text-align: left; padding: 0.75rem 1rem; font-size: 0.75rem; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em; border-bottom: 1px solid var(--surface-border); }
    .table td { padding: 0.75rem 1rem; font-size: 0.875rem; color: var(--text-primary); border-bottom: 1px solid var(--surface-border); }
    .table tr:last-child td { border-bottom: none; }
    .table tr:hover { background: var(--surface-hover); }
    .categoria-pai td { background: var(--color-primary-tint); }
    .cell-sub { padding-left: 2.5rem; color: var(--text-secondary); }
    .cell-actions { display: flex; gap: 0.25rem; justify-content: flex-end; }
    .action-btn {
      background: none; border: 1px solid var(--surface-border); border-radius: var(--radius-md);
      padding: 0.375rem; display: flex; color: var(--text-muted); cursor: pointer;
      transition: all var(--transition-fast);
    }
    .action-btn:hover { border-color: var(--color-primary); color: var(--color-primary); }
    .action-btn--danger:hover { border-color: var(--color-error); color: var(--color-error); }
    .badge-cat, .badge-sub { display: inline-flex; padding: 0.125rem 0.5rem; border-radius: 999px; font-size: 0.6875rem; font-weight: 500; }
    .badge-cat { background: var(--color-primary-tint); color: var(--color-primary); }
    .badge-sub { background: var(--surface-hover); color: var(--text-muted); }
    .field { display: flex; flex-direction: column; gap: 0.375rem; }
    .field label { font-size: 0.8125rem; font-weight: 500; color: var(--text-secondary); }
    .input, .select {
      padding: 0.625rem 0.75rem; border: 1px solid var(--surface-border); border-radius: var(--radius-md);
      font-size: 0.875rem; color: var(--text-primary); background: var(--content-surface);
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    }
    .input:focus, .select:focus { outline: none; border-color: var(--color-primary); box-shadow: 0 0 0 3px var(--color-primary-focus-ring); }
    .select {
      appearance: none;
      background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 24 24' fill='none' stroke='%2394a3b8' stroke-width='2' stroke-linecap='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E");
      background-repeat: no-repeat; background-position: right 0.75rem center; padding-right: 2.5rem; cursor: pointer;
    }
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
      .table th { display: none; }
      .table td { display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 1rem; }
      .table td::before { content: attr(data-label); font-weight: 600; font-size: 0.75rem; color: var(--text-muted); }
      .table tr { display: block; border: 1px solid var(--surface-border); border-radius: var(--radius-lg); margin-bottom: 0.5rem; }
      .table tr:hover { background: transparent; }
    }
  `]
})
export class CategoriasComponent implements OnInit {
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private repoReceita = inject(CategoriaReceitaRepository);
  private repoDespesa = inject(CategoriaDespesaRepository);

  tabs: Tab[] = [
    { id: 'receita', label: 'Receita' },
    { id: 'despesa', label: 'Despesa' }
  ];
  tabAtiva = signal('receita');
  items = signal<Categoria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Categoria | null>(null);
  salvando = signal(false);
  form: CategoriaRequest = { nome: '', categoriaPaiId: undefined };

  ngOnInit() { this.carregar(); }

  private get repo() {
    return this.tabAtiva() === 'receita' ? this.repoReceita : this.repoDespesa;
  }

  trocarAba(tab: string) {
    this.tabAtiva.set(tab);
    this.carregar();
  }

  async carregar() {
    this.loading.set(true);
    try {
      this.items.set(await firstValueFrom(this.repo.listar(this.auth.user()!.usuarioId)));
    } catch { this.notify.error('Erro ao carregar categorias'); }
    finally { this.loading.set(false); }
  }

  categoriasPai() { return this.items().filter(c => !c.categoriaPaiId); }
  subcategoriasDe(paiId: string) { return this.items().filter(c => c.categoriaPaiId === paiId); }

  abrirModal(item?: Categoria, paiId?: string) {
    this.form = {
      nome: item?.nome ?? '',
      categoriaPaiId: item ? item.categoriaPaiId : (paiId ?? undefined)
    };
    this.editando.set(item ?? null);
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome) { this.notify.error('Informe o nome da categoria'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Categoria atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.auth.user()!.usuarioId, this.form));
        this.notify.success('Categoria criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch { this.notify.error('Erro ao salvar categoria'); }
    finally { this.salvando.set(false); }
  }

  async excluir(item: Categoria) {
    const ok = await this.confirmService.confirm('Excluir categoria', `Deseja excluir "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Categoria excluída');
      await this.carregar();
    } catch { this.notify.error('Erro ao excluir categoria'); }
  }
}
