import { Component, input, output, signal, effect, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalComponent } from './modal.component';
import { Categoria } from '../../core/models/categoria.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';

export interface LancamentoForm {
  descricao: string;
  valor: number;
  data: string;
  idConta: string;
  idCategoria: string;
  idSubcategoria?: string;
  repete: boolean;
  dia?: number;
  diaUtil?: boolean;
  dataFim?: string;
}

interface LancamentoItem {
  descricao: string;
  valor: number;
  data: string;
  idConta: string;
  idCategoria: string;
  idSubcategoria?: string;
}

@Component({
  selector: 'app-lancamento-modal',
  standalone: true,
  imports: [FormsModule, ModalComponent],
  template: `
    <app-modal
      [visible]="visible()"
      [title]="title()"
      [saving]="salvando()"
      (visibleChange)="fechar()"
      (save)="salvar()"
    >
      <div class="field">
        <label>Descrição</label>
        <input [(ngModel)]="form.descricao" placeholder="Ex: Salário / Pizza" class="input" />
      </div>
      <div class="field-row">
        <div class="field">
          <label>Valor (R$)</label>
          <input type="number" step="0.01" [(ngModel)]="form.valor" (ngModelChange)="calcularPreview()" placeholder="0,00" class="input" />
        </div>
        <div class="field">
          <label>Data</label>
          <input type="date" [(ngModel)]="form.data" (ngModelChange)="calcularPreview()" class="input" />
        </div>
      </div>
      <div class="field">
        <label>Conta</label>
        <select [(ngModel)]="form.idConta" class="select">
          <option value="">Selecione...</option>
          @for (c of contas(); track c.id) {
            <option [value]="c.id">{{ c.nome }} ({{ c.banco }})</option>
          }
        </select>
      </div>
      <div class="field">
        <label>Categoria</label>
        <select [(ngModel)]="form.idCategoria" (ngModelChange)="onCategoriaChange()" class="select">
          <option value="">Selecione...</option>
          @for (cat of categoriasPai(); track cat.id) {
            <option [value]="cat.id">{{ cat.nome }}</option>
          }
        </select>
      </div>
      @if (subcategorias().length > 0) {
        <div class="field">
          <label>Subcategoria</label>
          <select [(ngModel)]="form.idSubcategoria" class="select">
            <option value="">—</option>
            @for (sub of subcategorias(); track sub.id) {
              <option [value]="sub.id">{{ sub.nome }}</option>
            }
          </select>
        </div>
      }

      <div class="repete-toggle">
        <label class="checkbox-label">
          <input type="checkbox" [(ngModel)]="form.repete" (ngModelChange)="calcularPreview()" class="checkbox" />
          <span>Repete mensalmente?</span>
        </label>
      </div>

      @if (form.repete) {
        <div class="field-row">
          <div class="field">
            <label>Dia</label>
            <input type="number" [min]="1" [max]="form.diaUtil ? 5 : 31" [(ngModel)]="form.dia" (ngModelChange)="calcularPreview()" class="input" />
          </div>
          <div class="field" style="justify-content: flex-end; padding-bottom: 0.375rem;">
            <label class="checkbox-label">
              <input type="checkbox" [(ngModel)]="form.diaUtil" class="checkbox" />
              <span>Dia útil</span>
            </label>
          </div>
        </div>
        <div class="field-row">
          <div class="field">
            <label>Data fim</label>
            <input type="date" [(ngModel)]="form.dataFim" (ngModelChange)="calcularPreview()" class="input" />
          </div>
        </div>
        @if (previewMeses() > 0) {
          <div class="preview">Serão gerados <strong>{{ previewMeses() }}</strong> lançamentos mensais</div>
        }
      }
    </app-modal>
  `,
  styles: [`
    .field { display: flex; flex-direction: column; gap: 0.375rem; }
    .field-row { display: flex; gap: 1rem; }
    .field-row .field { flex: 1; }
    .field label { font-size: 0.8125rem; font-weight: 500; color: var(--text-secondary); }
    .input, .select { padding: 0.625rem 0.75rem; border: 1px solid var(--surface-border); border-radius: var(--radius-md); font-size: 0.875rem; color: var(--text-primary); background: var(--content-surface); transition: border-color var(--transition-fast), box-shadow var(--transition-fast); }
    .input:focus, .select:focus { outline: none; border-color: var(--color-primary); box-shadow: 0 0 0 3px var(--color-primary-focus-ring); }
    .select { appearance: none; background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 24 24' fill='none' stroke='%2394a3b8' stroke-width='2' stroke-linecap='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right 0.75rem center; padding-right: 2.5rem; cursor: pointer; }
    .preview { padding: 0.625rem 0.75rem; background: var(--color-primary-tint); color: var(--color-primary); border-radius: var(--radius-md); font-size: 0.8125rem; text-align: center; }
    .checkbox-label { display: flex; align-items: center; gap: 0.375rem; font-size: 0.8125rem; color: var(--text-secondary); cursor: pointer; }
    .checkbox { width: 1rem; height: 1rem; accent-color: var(--color-primary); }
    .repete-toggle { padding: 0.25rem 0; }
  `]
})
export class LancamentoModalComponent {
  private notify = inject(NotificationService);

  visible = input(false);
  editando = input<LancamentoItem | null>(null);
  tipoLabel = input('receita');
  categorias = input<Categoria[]>([]);
  contas = input<ContaBancaria[]>([]);
  salvando = input(false);

  visibleChange = output<boolean>();
  saved = output<LancamentoForm>();

  form: LancamentoForm = this.emptyForm();
  previewMeses = signal(0);

  constructor() {
    effect(() => {
      const ini = this.editando();
      const visible = this.visible();
      if (visible) {
        if (ini) {
          this.form = {
            descricao: ini.descricao,
            valor: ini.valor,
            data: ini.data?.split('T')[0] ?? '',
            idConta: ini.idConta,
            idCategoria: ini.idCategoria,
            idSubcategoria: ini.idSubcategoria ?? undefined,
            repete: false,
            dia: 1,
            diaUtil: false,
            dataFim: ''
          };
        } else {
          const hoje = new Date().toISOString().split('T')[0];
          this.form = this.emptyForm();
          this.form.data = hoje;
        }
        this.calcularPreview();
      }
    });
  }

  categoriasPai() {
    return this.categorias().filter(c => !c.categoriaPaiId);
  }

  subcategorias() {
    return this.categorias().filter(c => c.categoriaPaiId === this.form.idCategoria);
  }

  onCategoriaChange() {
    this.form.idSubcategoria = undefined;
  }

  calcularPreview() {
    const ini = this.form.data;
    const fim = this.form.dataFim;
    if (ini && this.form.repete) {
      const inicio = new Date(ini + 'T00:00:00');
      const final = fim ? new Date(fim + 'T00:00:00') : inicio;
      let meses = (final.getFullYear() - inicio.getFullYear()) * 12 + (final.getMonth() - inicio.getMonth());
      this.previewMeses.set(meses > 0 ? meses : 1);
    } else {
      this.previewMeses.set(0);
    }
  }

  title() {
    return this.editando() ? `Editar ${this.tipoLabel()}` : `Nova ${this.tipoLabel()}`;
  }

  fechar() { this.visibleChange.emit(false); }

  salvar() {
    if (!this.form.descricao) { this.notify.error('Descrição é obrigatória'); return; }
    if (this.form.valor == null || isNaN(this.form.valor) || this.form.valor <= 0) { this.notify.error('Valor deve ser maior que zero'); return; }
    if (!this.form.idConta) { this.notify.error('Conta é obrigatória'); return; }
    if (!this.form.idCategoria) { this.notify.error('Categoria é obrigatória'); return; }
    if (!this.form.data) { this.notify.error('Data é obrigatória'); return; }

    if (this.form.repete) {
      if (!this.form.dia || this.form.dia < 1 || this.form.dia > 31) { this.notify.error('Dia deve estar entre 1 e 31'); return; }
      if (this.form.diaUtil && this.form.dia > 5) { this.notify.error('Dia útil deve estar entre 1 e 5'); return; }
      if (!this.form.dataFim) { this.notify.error('Data fim é obrigatória para recorrência'); return; }
    }

    this.saved.emit({ ...this.form });
  }

  private emptyForm(): LancamentoForm {
    return {
      descricao: '', valor: 0, data: '', idConta: '', idCategoria: '',
      repete: false, dia: 1, diaUtil: false, dataFim: ''
    };
  }
}
