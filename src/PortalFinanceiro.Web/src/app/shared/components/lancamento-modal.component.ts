import { Component, input, output, signal, effect, inject, computed } from '@angular/core';
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
  id?: string;
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

  form = signal<LancamentoForm>(this.emptyForm());
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
          this.form.set({
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
          });
        } else {
          const hoje = new Date().toISOString().split('T')[0];
          this.form.set({ ...this.emptyForm(), data: hoje });
        }
      }
    });
  }

  private atualizarCategorizacao() {
    const cats = this.categorias();
    const paiId = this.form().idCategoria;
    this.categoriasOptions.set(
      cats.filter(c => !c.categoriaPaiId).map(c => ({ value: c.id, label: c.nome }))
    );
    this.subcategoriasOptions.set(
      cats.filter(c => c.categoriaPaiId === paiId).map(c => ({ value: c.id, label: c.nome }))
    );
  }

  onCategoriaChange(value: string) {
    this.form.update(f => ({ ...f, idCategoria: value, idSubcategoria: undefined }));
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
    const f = this.form();
    switch (this.passoAtual()) {
      case 0:
        if (!f.idCategoria) errors['idCategoria'] = 'Categoria é obrigatória';
        break;
      case 1:
        if (!f.descricao) errors['descricao'] = 'Descrição é obrigatória';
        if (f.valor == null || isNaN(f.valor) || f.valor <= 0) errors['valor'] = 'Valor deve ser maior que zero';
        if (!f.data) errors['data'] = 'Data é obrigatória';
        break;
      case 2:
        if (!f.idConta) errors['idConta'] = 'Conta é obrigatória';
        if (f.repete) {
          if (!f.dia || f.dia < 1 || f.dia > 31) {
            errors['dia'] = 'Dia deve estar entre 1 e 31';
          } else if (f.diaUtil && f.dia > 5) {
            errors['dia'] = 'Dia útil deve estar entre 1 e 5';
          }
          if (!f.dataFim) {
            errors['dataFim'] = 'Data fim é obrigatória';
          } else if (f.data && f.dataFim <= f.data) {
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
    const f = this.form();
    if (indice === 0) return !!f.idCategoria;
    if (indice === 1) return !!(f.descricao?.trim() && f.data && f.valor > 0);
    return !!f.idConta;
  }

  voltar() {
    if (this.passoAtual() > 0) {
      this.fieldErrors.set({});
      this.passoAtual.set(this.passoAtual() - 1);
    }
  }

  selecionarSubcategoria(value: string) {
    const subId = value || undefined;
    const label = this.subcategoriasOptions().find(o => o.value === value)?.label;
    let descricaoSet = false;
    this.form.update(f => {
      const shouldSetDescricao = !!(label && !f.descricao?.trim());
      if (shouldSetDescricao) descricaoSet = true;
      return {
        ...f,
        idSubcategoria: subId,
        descricao: shouldSetDescricao ? label : f.descricao
      };
    });
    if (descricaoSet) {
      this.clearError('descricao');
    }
  }

  onDiaUtilChange() {
    const f = this.form();
    if (f.diaUtil && f.dia && f.dia > 5) {
      this.form.update(form => ({ ...form, dia: 5 }));
    }
    this.calcularPreview();
  }

  calcularPreview() {
    const f = this.form();
    const ini = f.data;
    const fim = f.dataFim;
    if (ini && f.repete && fim) {
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
    return this.editando()?.id ? `Editar ${this.tipoLabel()}` : `Nova ${this.tipoLabel()}`;
  }

  fechar() { this.visibleChange.emit(false); }

  salvar() {
    const f = this.form();
    const errors: Record<string, string> = {};

    if (!f.descricao) errors['descricao'] = 'Descrição é obrigatória';
    if (f.valor == null || isNaN(f.valor) || f.valor <= 0) errors['valor'] = 'Valor deve ser maior que zero';
    if (!f.idConta) errors['idConta'] = 'Conta é obrigatória';
    if (!f.idCategoria) errors['idCategoria'] = 'Categoria é obrigatória';
    if (!f.data) errors['data'] = 'Data é obrigatória';

    if (f.repete) {
      if (!f.dia || f.dia < 1 || f.dia > 31) {
        errors['dia'] = 'Dia deve estar entre 1 e 31';
      } else if (f.diaUtil && f.dia > 5) {
        errors['dia'] = 'Dia útil deve estar entre 1 e 5';
      }
      if (!f.dataFim) {
        errors['dataFim'] = 'Data fim é obrigatória';
      } else if (f.data && f.dataFim <= f.data) {
        errors['dataFim'] = 'Data fim deve ser posterior à data início';
      }
    }

    if (Object.keys(errors).length > 0) {
      this.fieldErrors.set(errors);
      this.notify.error('Corrija os campos destacados');
      return;
    }

    this.fieldErrors.set({});
    this.saved.emit({ ...f });
  }

  updateFormField<K extends keyof LancamentoForm>(key: K, value: LancamentoForm[K]) {
    this.form.update(f => ({ ...f, [key]: value }));
  }

  private emptyForm(): LancamentoForm {
    return {
      descricao: '', valor: 0, data: '', idConta: '', idCategoria: '',
      repete: false, dia: 1, diaUtil: false, dataFim: ''
    };
  }
}
