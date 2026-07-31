import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { ContaBancaria, ContaBancariaRequest } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-contas',
  standalone: true,
  imports: [FormsModule, ModalComponent, StatusBadgeComponent, CustomSelectComponent, LucideDynamicIcon],
  template: `
    <div class="page">
      <header class="page__header">
        <div class="page__header-left">
          <svg lucideIcon="wallet" class="page__icon" [size]="22" />
          <div>
            <h1 class="page__title">Contas Bancárias</h1>
            <p class="page__subtitle">Gerencie suas contas PF e PJ</p>
          </div>
        </div>
        <button class="add-btn" (click)="abrirModal()">
          <svg lucideIcon="plus" [size]="16" />
          Nova conta
        </button>
      </header>

      @if (loading()) {
        <div class="table-card">
          @for (i of [1,2,3,4]; track i) {
            <div class="skeleton-row">
              <div class="skeleton-line" style="width: 30%"></div>
              <div class="skeleton-line" style="width: 20%"></div>
              <div class="skeleton-line" style="width: 10%"></div>
            </div>
          }
        </div>
      } @else if (contas().length === 0) {
        <div class="empty-state">
          <svg lucideIcon="wallet" [size]="48" class="empty-icon" />
          <h3>Nenhuma conta cadastrada</h3>
          <p>Cadastre sua primeira conta bancária para começar.</p>
          <button class="add-btn" (click)="abrirModal()">
            <svg lucideIcon="plus" [size]="16" />
            Nova conta
          </button>
        </div>
      } @else {
        <div class="table-card">
          <table class="table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Banco</th>
                <th>Tipo</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (c of contas(); track c.id) {
                <tr>
                  <td class="cell-name" data-label="Nome">{{ c.nome }}</td>
                  <td data-label="Banco">{{ c.banco }}</td>
                  <td data-label="Tipo"><app-status-badge [type]="c.tipo === 'Pf' ? 'ativo' : 'inativo'" [label]="c.tipo === 'Pf' ? 'PF' : 'PJ'" /></td>
                  <td class="cell-actions">
                    <button class="action-btn" title="Editar" (click)="abrirModal(c)">
                      <svg lucideIcon="pencil" [size]="16" />
                    </button>
                    <button class="action-btn action-btn--danger" title="Excluir" (click)="excluir(c)">
                      <svg lucideIcon="trash-2" [size]="16" />
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>

    <app-modal
      [visible]="modalVisible()"
      [title]="editando() ? 'Editar conta' : 'Nova conta'"
      [saving]="salvando()"
      (visibleChange)="fecharModal()"
      (save)="salvar()"
    >
      <div class="field">
        <label>Nome</label>
        <input [(ngModel)]="form.nome" placeholder="Ex: Nubank PF" class="input" />
      </div>
      <div class="field">
        <label>Banco</label>
        <input [(ngModel)]="form.banco" placeholder="Ex: Nubank" class="input" />
      </div>
      <div class="field">
        <app-custom-select label="Tipo" placeholder="Selecione o tipo" [options]="tipoOptions" [value]="form.tipo" (valueChange)="form.tipo = $event" />
      </div>
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
    .cell-name { font-weight: 500; }
    .cell-actions { display: flex; gap: 0.25rem; justify-content: flex-end; }
    .action-btn {
      background: none; border: 1px solid var(--surface-border); border-radius: var(--radius-md);
      padding: 0.375rem; display: flex; color: var(--text-muted); cursor: pointer;
      transition: all var(--transition-fast);
    }
    .action-btn:hover { border-color: var(--color-primary); color: var(--color-primary); }
    .action-btn--danger:hover { border-color: var(--color-error); color: var(--color-error); }
    .field { display: flex; flex-direction: column; gap: 0.375rem; }
    .field label { font-size: 0.8125rem; font-weight: 500; color: var(--text-secondary); }
    .input {
      padding: 0.625rem 0.75rem; border: 1px solid var(--surface-border); border-radius: var(--radius-md);
      font-size: 0.875rem; color: var(--text-primary); background: var(--content-surface);
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    }
    .input:focus { outline: none; border-color: var(--color-primary); box-shadow: 0 0 0 3px var(--color-primary-focus-ring); }
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
export class ContasComponent implements OnInit {
  private repo = inject(ContaBancariaRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  contas = signal<ContaBancaria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<ContaBancaria | null>(null);
  salvando = signal(false);

  form: ContaBancariaRequest = { nome: '', banco: '', tipo: 'Pf' };

  tipoOptions: SelectOption[] = [
    { value: 'Pf', label: 'Pessoa Física' },
    { value: 'Pj', label: 'Pessoa Jurídica' },
  ];

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar(this.auth.user()!.usuarioId));
      this.contas.set(data);
    } catch { this.notify.error('Erro ao carregar contas'); }
    finally { this.loading.set(false); }
  }

  abrirModal(item?: ContaBancaria) {
    if (item) {
      this.form = { nome: item.nome, banco: item.banco, tipo: item.tipo };
      this.editando.set(item);
    } else {
      this.form = { nome: '', banco: '', tipo: 'Pf' };
      this.editando.set(null);
    }
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome || !this.form.banco) { this.notify.error('Preencha todos os campos'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Conta atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.auth.user()!.usuarioId, this.form));
        this.notify.success('Conta criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch { this.notify.error('Erro ao salvar conta'); }
    finally { this.salvando.set(false); }
  }

  async excluir(conta: ContaBancaria) {
    const ok = await this.confirmService.confirm('Excluir conta', `Deseja excluir "${conta.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(conta.id));
      this.notify.success('Conta excluída');
      await this.carregar();
    } catch { this.notify.error('Erro ao excluir conta'); }
  }
}
