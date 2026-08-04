import { Component, input, output, signal, effect, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';
import { ModalComponent } from './modal.component';
import { CustomSelectComponent, SelectOption } from './custom-select.component';
import { CurrencyInputDirective } from '../directives/currency-input.directive';
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
  imports: [FormsModule, ModalComponent, CustomSelectComponent, CurrencyInputDirective, LucideDynamicIcon],
  template: `
    <app-modal
      [visible]="visible()"
      [title]="title()"
      [saving]="salvando()"
      (visibleChange)="fechar()"
      (save)="salvar()"
    >
      <!-- DADOS GERAIS -->
      <div class="section">
        <div class="section-header">
          <svg lucideIcon="receipt" [size]="16" />
          <span>Dados Gerais</span>
        </div>
        <div class="field">
          <label>Descrição</label>
          <input
            [(ngModel)]="form.descricao"
            placeholder="Ex: Salário / Pizza"
            class="input"
            [class.input-error]="fieldErrors()['descricao']"
            (ngModelChange)="clearError('descricao')"
          />
          @if (fieldErrors()['descricao']) {
            <span class="field-error">{{ fieldErrors()['descricao'] }}</span>
          }
        </div>
        <div class="grid-2">
          <div class="field">
            <label>Valor (R$)</label>
            <input
              type="text"
              currencyInput
              [(ngModel)]="form.valor"
              (ngModelChange)="calcularPreview(); clearError('valor')"
              placeholder="0,00"
              class="input"
              [class.input-error]="fieldErrors()['valor']"
              inputmode="decimal"
            />
            @if (fieldErrors()['valor']) {
              <span class="field-error">{{ fieldErrors()['valor'] }}</span>
            }
          </div>
          <div class="field">
            <label>Data</label>
            <input
              type="date"
              lang="pt-BR"
              [(ngModel)]="form.data"
              (ngModelChange)="calcularPreview(); clearError('data')"
              class="input"
              [class.input-error]="fieldErrors()['data']"
            />
            <span class="field-hint">DD/MM/AAAA</span>
            @if (fieldErrors()['data']) {
              <span class="field-error">{{ fieldErrors()['data'] }}</span>
            }
          </div>
        </div>
        <div class="field">
          <label>Conta</label>
          <app-custom-select
            placeholder="Selecione a conta"
            [options]="contasOptions()"
            [value]="form.idConta"
            (valueChange)="form.idConta = $event; clearError('idConta')"
          />
          @if (fieldErrors()['idConta']) {
            <span class="field-error">{{ fieldErrors()['idConta'] }}</span>
          }
        </div>
      </div>

      <!-- CATEGORIZAÇÃO -->
      <div class="section">
        <div class="section-header">
          <svg lucideIcon="tag" [size]="16" />
          <span>Categorização</span>
        </div>
        <div class="grid-2">
          <div class="field">
            <label>Categoria</label>
            <app-custom-select
              placeholder="Selecione"
              [options]="categoriasOptions()"
              [value]="form.idCategoria"
              (valueChange)="form.idCategoria = $event; onCategoriaChange(); clearError('idCategoria')"
            />
            @if (fieldErrors()['idCategoria']) {
              <span class="field-error">{{ fieldErrors()['idCategoria'] }}</span>
            }
          </div>
          @if (subcategoriasOptions().length > 0) {
            <div class="field">
              <label>Subcategoria</label>
              <app-custom-select
                placeholder="Nenhuma"
                [options]="subcategoriasOptions()"
                [value]="form.idSubcategoria ?? ''"
                (valueChange)="form.idSubcategoria = $event || undefined"
              />
            </div>
          }
        </div>
      </div>

      <!-- RECORRÊNCIA -->
      <div class="repete-toggle">
        <label class="checkbox-label">
          <input type="checkbox" [(ngModel)]="form.repete" (ngModelChange)="calcularPreview()" class="checkbox" />
          <svg lucideIcon="repeat" [size]="14" />
          <span>Repete mensalmente?</span>
        </label>
      </div>

      @if (form.repete) {
        <div class="section section--recorrencia">
          <div class="section-header">
            <svg lucideIcon="calendar-clock" [size]="16" />
            <span>Recorrência</span>
          </div>
          <div class="grid-3">
            <div class="field">
              <label>Dia do mês</label>
              <input
                type="number"
                [min]="1"
                [max]="form.diaUtil ? 5 : 31"
                [(ngModel)]="form.dia"
                (ngModelChange)="calcularPreview(); clearError('dia')"
                placeholder="1-31"
                class="input"
                [class.input-error]="fieldErrors()['dia']"
              />
              @if (fieldErrors()['dia']) {
                <span class="field-error">{{ fieldErrors()['dia'] }}</span>
              }
            </div>
            <div class="field field--checkbox">
              <label class="checkbox-label">
                <input
                  type="checkbox"
                  [(ngModel)]="form.diaUtil"
                  (ngModelChange)="onDiaUtilChange()"
                  class="checkbox"
                />
                <span>Dia útil</span>
              </label>
              <span class="field-hint">Limita ao 1º dia útil</span>
            </div>
            <div class="field">
              <label>Data fim</label>
              <input
                type="date"
                lang="pt-BR"
                [(ngModel)]="form.dataFim"
                (ngModelChange)="calcularPreview(); clearError('dataFim')"
                class="input"
                [class.input-error]="fieldErrors()['dataFim']"
              />
              <span class="field-hint">DD/MM/AAAA</span>
              @if (fieldErrors()['dataFim']) {
                <span class="field-error">{{ fieldErrors()['dataFim'] }}</span>
              }
            </div>
          </div>
          @if (previewMeses() > 0) {
            <div class="preview">
              <svg lucideIcon="info" [size]="14" />
              Serão gerados <strong>{{ previewMeses() }}</strong> lançamentos mensais
            </div>
          }
        </div>
      }
    </app-modal>
  `,
  styles: [`
    .section {
      border: 1px solid var(--surface-border);
      border-radius: var(--radius-lg);
      padding: 1rem;
      margin-bottom: 0.75rem;
    }
    .section--recorrencia {
      border-color: var(--color-primary-muted);
      background: var(--color-primary-tint);
    }
    .section-header {
      display: flex;
      align-items: center;
      gap: 0.375rem;
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 0.75rem;
      padding-bottom: 0.5rem;
      border-bottom: 1px solid var(--surface-border);
    }
    .section--recorrencia .section-header {
      color: var(--color-primary);
      border-bottom-color: var(--color-primary-muted);
    }
    .field { display: flex; flex-direction: column; gap: 0.25rem; }
    .field--checkbox { justify-content: center; }
    .field label { font-size: 0.8125rem; font-weight: 500; color: var(--text-secondary); }
    .field-hint { font-size: 0.6875rem; color: var(--text-muted); }
    .field-error { font-size: 0.75rem; color: var(--color-error); }
    .input-error { border-color: var(--color-error) !important; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
    .grid-3 { display: grid; grid-template-columns: 1fr auto 1fr; gap: 0.75rem; align-items: start; }
    .input, .select {
      padding: 0.625rem 0.75rem; border: 1px solid var(--surface-border);
      border-radius: var(--radius-md); font-size: 0.875rem;
      color: var(--text-primary); background: var(--content-surface);
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
      width: 100%;
    }
    .input:focus, .select:focus {
      outline: none; border-color: var(--color-primary);
      box-shadow: 0 0 0 3px var(--color-primary-focus-ring);
    }
    .preview {
      display: flex; align-items: center; gap: 0.5rem;
      padding: 0.625rem 0.75rem; background: var(--color-primary-tint);
      color: var(--color-primary); border-radius: var(--radius-md);
      font-size: 0.8125rem; margin-top: 0.75rem;
    }
    .checkbox-label {
      display: flex; align-items: center; gap: 0.375rem;
      font-size: 0.8125rem; color: var(--text-secondary); cursor: pointer;
    }
    .checkbox { width: 1rem; height: 1rem; accent-color: var(--color-primary); }
    .repete-toggle { padding: 0.25rem 0; margin-bottom: 0.5rem; }
    @media (max-width: 480px) {
      .grid-2, .grid-3 { grid-template-columns: 1fr; }
    }
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
  fieldErrors = signal<Record<string, string>>({});

  contasOptions = signal<SelectOption[]>([]);
  categoriasOptions = signal<SelectOption[]>([]);
  subcategoriasOptions = signal<SelectOption[]>([]);

  constructor() {
    effect(() => {
      const contas = this.contas();
      this.contasOptions.set(contas.map(c => ({ value: c.id, label: `${c.nome} (${c.banco})` })));
    });

    effect(() => {
      const cats = this.categorias();
      const paiId = this.form.idCategoria;
      this.categoriasOptions.set(
        cats.filter(c => !c.categoriaPaiId).map(c => ({ value: c.id, label: c.nome }))
      );
      this.subcategoriasOptions.set(
        cats.filter(c => c.categoriaPaiId === paiId).map(c => ({ value: c.id, label: c.nome }))
      );
    });

    effect(() => {
      const ini = this.editando();
      const visible = this.visible();
      if (visible) {
        this.fieldErrors.set({});
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

  onCategoriaChange() {
    this.form.idSubcategoria = undefined;
  }

  onDiaUtilChange() {
    if (this.form.diaUtil && this.form.dia && this.form.dia > 5) {
      this.form.dia = 5;
    }
    this.calcularPreview();
  }

  calcularPreview() {
    const ini = this.form.data;
    const fim = this.form.dataFim;
    if (ini && this.form.repete && fim) {
      const inicio = new Date(ini + 'T00:00:00');
      const final = new Date(fim + 'T00:00:00');
      if (final <= inicio) {
        this.previewMeses.set(0);
        return;
      }
      let meses = (final.getFullYear() - inicio.getFullYear()) * 12 + (final.getMonth() - inicio.getMonth());
      this.previewMeses.set(meses > 0 ? meses : 1);
    } else {
      this.previewMeses.set(0);
    }
  }

  clearError(field: string) {
    const errors = { ...this.fieldErrors() };
    delete errors[field];
    this.fieldErrors.set(errors);
  }

  title() {
    return this.editando() ? `Editar ${this.tipoLabel()}` : `Nova ${this.tipoLabel()}`;
  }

  fechar() { this.visibleChange.emit(false); }

  salvar() {
    const errors: Record<string, string> = {};

    if (!this.form.descricao) errors['descricao'] = 'Descrição é obrigatória';
    if (this.form.valor == null || isNaN(this.form.valor) || this.form.valor <= 0) errors['valor'] = 'Valor deve ser maior que zero';
    if (!this.form.idConta) errors['idConta'] = 'Conta é obrigatória';
    if (!this.form.idCategoria) errors['idCategoria'] = 'Categoria é obrigatória';
    if (!this.form.data) errors['data'] = 'Data é obrigatória';

    if (this.form.repete) {
      if (!this.form.dia || this.form.dia < 1 || this.form.dia > 31) {
        errors['dia'] = 'Dia deve estar entre 1 e 31';
      } else if (this.form.diaUtil && this.form.dia > 5) {
        errors['dia'] = 'Dia útil deve estar entre 1 e 5';
      }
      if (!this.form.dataFim) {
        errors['dataFim'] = 'Data fim é obrigatória';
      } else if (this.form.data && this.form.dataFim <= this.form.data) {
        errors['dataFim'] = 'Data fim deve ser posterior à data início';
      }
    }

    if (Object.keys(errors).length > 0) {
      this.fieldErrors.set(errors);
      this.notify.error('Corrija os campos destacados');
      return;
    }

    this.fieldErrors.set({});
    this.saved.emit({ ...this.form });
  }

  private emptyForm(): LancamentoForm {
    return {
      descricao: '', valor: 0, data: '', idConta: '', idCategoria: '',
      repete: false, dia: 1, diaUtil: false, dataFim: ''
    };
  }
}
