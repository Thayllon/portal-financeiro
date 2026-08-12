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
  geraDas?: boolean;
  percentualDas?: number;
}

interface LancamentoItem {
  descricao: string;
  valor: number;
  data: string;
  idConta: string;
  idCategoria: string;
  idSubcategoria?: string;
  geraDas?: boolean;
  percentualDas?: number;
}

@Component({
  selector: 'app-lancamento-modal',
  standalone: true,
  imports: [FormsModule, ModalComponent, CustomSelectComponent, CurrencyInputDirective, LucideDynamicIcon],
  templateUrl: './lancamento-modal.component.html',
  styleUrl: './lancamento-modal.component.scss'
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
  passoAtual = signal(0);

  contasOptions = signal<SelectOption[]>([]);
  categoriasOptions = signal<SelectOption[]>([]);
  subcategoriasOptions = signal<SelectOption[]>([]);

  constructor() {
    effect(() => {
      const contas = this.contas();
      this.contasOptions.set(contas.map(c => ({ value: c.id, label: `${c.nome} (${c.banco})` })));
    });

    effect(() => {
      this.atualizarCategorizacao();
    });

    effect(() => {
      const ini = this.editando();
      const visible = this.visible();
      if (visible) {
        this.fieldErrors.set({});
        this.passoAtual.set(0);
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
            dataFim: '',
            geraDas: ini.geraDas ?? false,
            percentualDas: ini.percentualDas ?? undefined
          };
        } else {
          const hoje = new Date().toISOString().split('T')[0];
          this.form = this.emptyForm();
          this.form.data = hoje;
        }
        this.atualizarCategorizacao();
        this.calcularPreview();
      }
    });
  }

  private atualizarCategorizacao() {
    const cats = this.categorias();
    const paiId = this.form.idCategoria;
    this.categoriasOptions.set(
      cats.filter(c => !c.categoriaPaiId).map(c => ({ value: c.id, label: c.nome }))
    );
    this.subcategoriasOptions.set(
      cats.filter(c => c.categoriaPaiId === paiId).map(c => ({ value: c.id, label: c.nome }))
    );
  }

  onCategoriaChange() {
    this.form.idSubcategoria = undefined;
    this.atualizarCategorizacao();
  }

  irPara(indice: number) {
    if (indice < this.passoAtual()) {
      this.fieldErrors.set({});
      this.passoAtual.set(indice);
      return;
    }
    while (this.passoAtual() < indice) {
      if (!this.avancar()) break;
    }
  }

  avancar(): boolean {
    if (!this.validarPassoAtual()) {
      this.notify.error('Preencha os campos destacados para avançar');
      return false;
    }
    if (this.passoAtual() < 2) {
      this.fieldErrors.set({});
      this.passoAtual.update(v => v + 1);
    }
    return true;
  }

  private validarPassoAtual(): boolean {
    const errors: Record<string, string> = {};
    switch (this.passoAtual()) {
      case 0:
        if (!this.form.idCategoria) errors['idCategoria'] = 'Categoria é obrigatória';
        break;
      case 1:
        if (!this.form.descricao) errors['descricao'] = 'Descrição é obrigatória';
        if (this.form.valor == null || isNaN(this.form.valor) || this.form.valor <= 0) errors['valor'] = 'Valor deve ser maior que zero';
        if (!this.form.data) errors['data'] = 'Data é obrigatória';
        break;
      case 2:
        if (!this.form.idConta) errors['idConta'] = 'Conta é obrigatória';
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
        break;
    }
    if (Object.keys(errors).length > 0) {
      this.fieldErrors.set(errors);
      return false;
    }
    return true;
  }

  passoConcluido(indice: number): boolean {
    if (indice === 0) return !!this.form.idCategoria;
    if (indice === 1) return !!(this.form.descricao?.trim() && this.form.data && this.form.valor > 0);
    return !!this.form.idConta;
  }

  voltar() {
    if (this.passoAtual() > 0) {
      this.fieldErrors.set({});
      this.passoAtual.set(this.passoAtual() - 1);
    }
  }

  selecionarSubcategoria(value: string) {
    this.form.idSubcategoria = value || undefined;
    const label = this.subcategoriasOptions().find(o => o.value === value)?.label;
    if (label && !this.form.descricao?.trim()) {
      this.form.descricao = label;
      this.clearError('descricao');
    }
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

    if (this.form.geraDas) {
      if (this.form.percentualDas == null || isNaN(this.form.percentualDas) || this.form.percentualDas <= 0 || this.form.percentualDas >= 100) {
        errors['percentualDas'] = 'Percentual deve estar entre 0 e 100';
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
      repete: false, dia: 1, diaUtil: false, dataFim: '',
      geraDas: false, percentualDas: 6
    };
  }
}
