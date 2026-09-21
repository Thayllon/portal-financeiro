import { Component, input, output, signal, effect, inject, computed, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { LucideDynamicIcon } from '@lucide/angular';
import { ModalComponent } from './modal.component';
import { CustomSelectComponent, SelectOption } from './custom-select.component';
import { CurrencyInputDirective } from '../directives/currency-input.directive';
import { Categoria } from '../../core/models/categoria.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { Pessoa } from '../../core/models/pessoa.model';
import { Parceria } from '../../core/models/parceria.model';
import { Contrato } from '../../core/models/contrato.model';
import { CategoriaReceitaRepository, CategoriaDespesaRepository, CategoriaServicoRepository } from '../../core/repositories/categoria.repository';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { NotificationService } from '../../core/services/notification.service';
import { mensagemErro } from '../../shared/utils/api-error.util';

export interface CategoriaServicoBloco {
  categoriaServicoId: string;
  subcategoriasSelecionadas: string[];
}

export interface ServicoItem {
  categoriaServicoId: string;
  subcategoriaServicoId?: string;
}

export interface LancamentoForm {
  descricao: string;
  valor: number;
  data: string;
  idConta: string;
  idCategoria: string;
  idSubcategoria?: string;
  idParceria?: string;
  idContrato?: string;
  categoriasServicoBloco: CategoriaServicoBloco[];
  servicos?: ServicoItem[];
  idCliente?: string;
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
  idParceiro?: string;
  idParceria?: string;
  idContrato?: string;
  servicos?: { categoriaServicoId: string; subcategoriaServicoId?: string }[];
  idCliente?: string;
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
  private catReceitaRepo = inject(CategoriaReceitaRepository);
  private catDespesaRepo = inject(CategoriaDespesaRepository);
  private catServicoRepo = inject(CategoriaServicoRepository);
  private pessoaRepo = inject(PessoaRepository);

  visible = input(false);
  editando = input<LancamentoItem | null>(null);
  tipoLabel = input('receita');
  dominioCategoria = input<'receita' | 'despesa'>('receita');
  categorias = input<Categoria[]>([]);
  contas = input<ContaBancaria[]>([]);
  fluxoAdicional = input(false);
  parceiros = input<Pessoa[]>([]);
  parcerias = input<Parceria[]>([]);
  contratos = input<Contrato[]>([]);
  clientes = input<Pessoa[]>([]);
  categoriasServico = input<Categoria[]>([]);
  salvando = input(false);

  visibleChange = output<boolean>();
  saved = output<LancamentoForm>();
  categoriaCriada = output<Categoria>();
  servicoCriado = output<Categoria>();
  clienteCriado = output<Pessoa>();

  form = signal<LancamentoForm>(this.emptyForm());
  previewMeses = signal(0);
  fieldErrors = signal<Record<string, string>>({});
  passoAtual = signal(0);
  vinculoSelecionado = signal<'contrato' | 'parceria' | null>(null);

  contasOptions = signal<SelectOption[]>([]);
  categoriasOptions = signal<SelectOption[]>([]);
  subcategoriasOptions = signal<SelectOption[]>([]);
  parceriasOptions = signal<SelectOption[]>([]);
  contratosOptions = signal<SelectOption[]>([]);
  clientesOptions = signal<SelectOption[]>([]);
  categoriasServicoPais = signal<SelectOption[]>([]);
  subcategoriasServicoMap = signal<Map<string, SelectOption[]>>(new Map());

  categoriasServicoDisponiveis = computed(() => {
    const usadas = new Set(this.form().categoriasServicoBloco.map(b => b.categoriaServicoId));
    return this.categoriasServicoPais().filter(o => !usadas.has(o.value));
  });

  ultimoPasso = computed(() => this.fluxoAdicional() ? 5 : 2);

  constructor() {
    effect(() => {
      const contas = this.contas();
      this.contasOptions.set(contas.map(c => ({ value: c.id, label: `${c.nome} (${c.banco})` })));
    });

    effect(() => {
      this.atualizarCategorizacao();
    });

    effect(() => {
      this.atualizarCategoriasServico();
    });

    effect(() => {
      const p = this.parcerias();
      this.parceriasOptions.set(p.map(x => ({ value: x.id, label: `${x.nome} (${x.parceiro} - ${x.cliente})` })));
    });

    effect(() => {
      const c = this.contratos();
      this.contratosOptions.set(c.map(x => ({ value: x.id, label: `${x.nome} (${x.cliente})` })));
    });

    effect(() => {
      const c = this.clientes();
      this.clientesOptions.set(c.map(x => ({ value: x.id, label: x.nome })));
    });

    effect(() => {
      const ini = this.editando();
      const visible = this.visible();
      if (visible) {
        this.fieldErrors.set({});
        this.passoAtual.set(0);
        if (ini) {
          const blocos = this.agruparServicosEmBlocos(ini.servicos ?? []);
          this.vinculoSelecionado.set(ini.idContrato ? 'contrato' : ini.idParceria ? 'parceria' : null);
          this.form.set({
            descricao: ini.descricao,
            valor: ini.valor,
            data: ini.data?.split('T')[0] ?? '',
            idConta: ini.idConta,
            idCategoria: ini.idCategoria,
            idSubcategoria: ini.idSubcategoria ?? undefined,
            idParceria: ini.idParceria ?? undefined,
            idContrato: ini.idContrato ?? undefined,
            categoriasServicoBloco: blocos,
            idCliente: ini.idCliente ?? undefined,
            repete: false,
            dia: 1,
            diaUtil: false,
            dataFim: ''
          });
        } else {
          const hoje = new Date().toISOString().split('T')[0];
          const contas = untracked(() => this.contas());
          const contaPadrao = contas.find(c => c.ehPadrao)?.id ?? contas[0]?.id ?? '';
          this.vinculoSelecionado.set(null);
          this.form.set({ ...this.emptyForm(), data: hoje, idConta: contaPadrao });
        }
      }
    });
  }

  private agruparServicosEmBlocos(servicos: { categoriaServicoId: string; subcategoriaServicoId?: string }[]): CategoriaServicoBloco[] {
    const mapa = new Map<string, Set<string>>();
    for (const s of servicos) {
      if (!mapa.has(s.categoriaServicoId)) mapa.set(s.categoriaServicoId, new Set());
      if (s.subcategoriaServicoId) mapa.get(s.categoriaServicoId)!.add(s.subcategoriaServicoId);
    }
    return Array.from(mapa.entries()).map(([catId, subs]) => ({
      categoriaServicoId: catId,
      subcategoriasSelecionadas: Array.from(subs)
    }));
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

  private atualizarCategoriasServico() {
    const cats = this.categoriasServico();
    const pais = cats.filter(c => !c.categoriaPaiId);
    this.categoriasServicoPais.set(pais.map(c => ({ value: c.id, label: c.nome })));
    const map = new Map<string, SelectOption[]>();
    for (const pai of pais) {
      const subs = cats.filter(c => c.categoriaPaiId === pai.id);
      map.set(pai.id, subs.map(s => ({ value: s.id, label: s.nome })));
    }
    this.subcategoriasServicoMap.set(map);
  }

  getSubcategoriasServico(categoriaId: string): SelectOption[] {
    return this.subcategoriasServicoMap().get(categoriaId) ?? [];
  }

  getCategoriaServicoNome(categoriaId: string): string {
    return this.categoriasServicoPais().find(o => o.value === categoriaId)?.label ?? '';
  }

  temSubcategorias(categoriaId: string): boolean {
    return this.getSubcategoriasServico(categoriaId).length > 0;
  }

  onCategoriaChange(value: string) {
    this.form.update(f => ({ ...f, idCategoria: value, idSubcategoria: undefined }));
    this.atualizarCategorizacao();
  }

  adicionarBlocoServico(categoriaId: string) {
    this.form.update(f => ({
      ...f,
      categoriasServicoBloco: [...f.categoriasServicoBloco, { categoriaServicoId: categoriaId, subcategoriasSelecionadas: [] }]
    }));
  }

  removerBlocoServico(index: number) {
    this.form.update(f => ({
      ...f,
      categoriasServicoBloco: f.categoriasServicoBloco.filter((_, i) => i !== index)
    }));
  }

  toggleSubcategoriaServico(blocoIndex: number, subcategoriaId: string) {
    this.form.update(f => {
      const blocos = [...f.categoriasServicoBloco];
      const bloco = { ...blocos[blocoIndex] };
      const idx = bloco.subcategoriasSelecionadas.indexOf(subcategoriaId);
      if (idx >= 0) {
        bloco.subcategoriasSelecionadas = bloco.subcategoriasSelecionadas.filter(s => s !== subcategoriaId);
      } else {
        bloco.subcategoriasSelecionadas = [...bloco.subcategoriasSelecionadas, subcategoriaId];
      }
      blocos[blocoIndex] = bloco;
      return { ...f, categoriasServicoBloco: blocos };
    });
  }

  isSubcategoriaSelected(blocoIndex: number, subcategoriaId: string): boolean {
    return this.form().categoriasServicoBloco[blocoIndex]?.subcategoriasSelecionadas.includes(subcategoriaId) ?? false;
  }

  flatttenServicos(): ServicoItem[] {
    const result: ServicoItem[] = [];
    for (const bloco of this.form().categoriasServicoBloco) {
      if (bloco.subcategoriasSelecionadas.length > 0) {
        for (const subId of bloco.subcategoriasSelecionadas) {
          result.push({ categoriaServicoId: bloco.categoriaServicoId, subcategoriaServicoId: subId });
        }
      } else {
        result.push({ categoriaServicoId: bloco.categoriaServicoId });
      }
    }
    return result;
  }

  selecionarVinculo(tipo: 'contrato' | 'parceria') {
    if (this.vinculoSelecionado() === tipo) {
      this.vinculoSelecionado.set(null);
      this.form.update(f => ({ ...f, idParceria: undefined, idContrato: undefined }));
      return;
    }
    this.vinculoSelecionado.set(tipo);
    this.form.update(f => ({ ...f, idParceria: undefined, idContrato: undefined }));
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
    if (this.passoAtual() < this.ultimoPasso()) {
      this.fieldErrors.set({});
      this.passoAtual.update(v => v + 1);
    }
    return true;
  }

  private validarPassoAtual(): boolean {
    const errors: Record<string, string> = {};
    const f = this.form();
    const passo = this.passoAtual();
    const fluxo = this.fluxoAdicional();

    if (fluxo) {
      switch (passo) {
        case 0:
          if (!f.idCategoria) errors['idCategoria'] = 'Categoria é obrigatória';
          break;
        case 1:
          break;
        case 2:
          if (f.categoriasServicoBloco.length === 0) {
            errors['servicos'] = 'Adicione pelo menos um serviço';
          } else {
            for (let i = 0; i < f.categoriasServicoBloco.length; i++) {
              const bloco = f.categoriasServicoBloco[i];
              if (this.temSubcategorias(bloco.categoriaServicoId) && bloco.subcategoriasSelecionadas.length === 0) {
                errors['servicos'] = `Selecione ao menos uma subcategoria no serviço ${i + 1}`;
                break;
              }
            }
          }
          break;
        case 3:
          if (!f.idCliente) errors['idCliente'] = 'Selecione um cliente';
          break;
        case 4:
          if (!f.descricao) errors['descricao'] = 'Descrição é obrigatória';
          if (f.valor == null || isNaN(f.valor) || f.valor <= 0) errors['valor'] = 'Valor deve ser maior que zero';
          if (!f.data) errors['data'] = 'Data é obrigatória';
          break;
        case 5:
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
    } else {
      switch (passo) {
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
    }

    if (Object.keys(errors).length > 0) {
      this.fieldErrors.set(errors);
      return false;
    }
    return true;
  }

  passoConcluido(indice: number): boolean {
    const f = this.form();
    const fluxo = this.fluxoAdicional();

    if (fluxo) {
      switch (indice) {
        case 0: return !!f.idCategoria;
        case 1: return !!f.idParceria || !!f.idContrato;
        case 2: return f.categoriasServicoBloco.length > 0;
        case 3: return !!f.idCliente;
        case 4: return !!(f.descricao?.trim() && f.data && f.valor > 0);
        case 5: return !!f.idConta;
        default: return false;
      }
    } else {
      switch (indice) {
        case 0: return !!f.idCategoria;
        case 1: return !!(f.descricao?.trim() && f.data && f.valor > 0);
        case 2: return !!f.idConta;
        default: return false;
      }
    }
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

  onClienteChange(value: string) {
    const id = value || undefined;
    const label = this.clientesOptions().find(o => o.value === value)?.label;
    let descricaoSet = false;
    this.form.update(f => {
      const shouldSetDescricao = this.devePuxarDescricaoCliente(f.descricao) && !!label;
      if (shouldSetDescricao) descricaoSet = true;
      return {
        ...f,
        idCliente: id,
        descricao: shouldSetDescricao && label ? label : f.descricao
      };
    });
    if (descricaoSet) {
      this.clearError('descricao');
    }
  }

  private devePuxarDescricaoCliente(descricaoAtual: string): boolean {
    return this.fluxoAdicional() && this.dominioCategoria() === 'receita' && !descricaoAtual?.trim();
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
      const meses = (final.getFullYear() - inicio.getFullYear()) * 12 + (final.getMonth() - inicio.getMonth());
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

  subtitleText() {
    return this.editando()?.id ? `Altere os dados da ${this.tipoLabel()}` : `Cadastre uma nova ${this.tipoLabel()} em poucos passos.`;
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

    if (this.fluxoAdicional()) {
      if (f.categoriasServicoBloco.length === 0) {
        errors['servicos'] = 'Adicione pelo menos um serviço';
      }
      if (!f.idCliente) errors['idCliente'] = 'Cliente é obrigatório';
    }

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
    this.saved.emit({ ...f, servicos: this.flatttenServicos() });
  }

  updateFormField<K extends keyof LancamentoForm>(key: K, value: LancamentoForm[K]) {
    this.form.update(f => ({ ...f, [key]: value }));
  }

  quickAdd = signal<'categoria' | 'servico' | 'cliente' | null>(null);
  quickNome = signal('');
  quickPaiId = signal('');
  quickTelefone = signal('');
  quickSalvando = signal(false);
  quickError = signal('');

  quickTitulo = computed(() => {
    switch (this.quickAdd()) {
      case 'categoria': return this.dominioCategoria() === 'despesa' ? 'Nova categoria de despesa' : 'Nova categoria de receita';
      case 'servico': return 'Nova categoria de serviço';
      case 'cliente': return 'Novo cliente';
      default: return '';
    }
  });

  quickPaisOptions = computed(() => {
    if (this.quickAdd() === 'servico') return this.categoriasServicoPais();
    return this.categoriasOptions();
  });

  abrirQuickAdd(tipo: 'categoria' | 'servico' | 'cliente') {
    this.quickNome.set('');
    this.quickPaiId.set('');
    this.quickTelefone.set('');
    this.quickError.set('');
    this.quickAdd.set(tipo);
  }

  fecharQuickAdd() {
    if (!this.quickSalvando()) this.quickAdd.set(null);
  }

  async salvarQuickAdd() {
    const nome = this.quickNome().trim();
    if (!nome) { this.quickError.set('Informe o nome'); return; }
    const tipo = this.quickAdd();
    if (!tipo) return;
    this.quickError.set('');
    this.quickSalvando.set(true);
    try {
      if (tipo === 'categoria') {
        const repo = this.dominioCategoria() === 'despesa' ? this.catDespesaRepo : this.catReceitaRepo;
        const paiId = this.quickPaiId() || undefined;
        const criada = await firstValueFrom(repo.criar({ nome, ...(paiId ? { categoriaPaiId: paiId } : {}) }));
        this.categoriaCriada.emit(criada);
        if (paiId) {
          this.form.update(f => ({ ...f, idCategoria: paiId, idSubcategoria: criada.id }));
          this.atualizarCategorizacao();
        } else {
          this.onCategoriaChange(criada.id);
        }
        this.clearError('idCategoria');
      } else if (tipo === 'servico') {
        const paiId = this.quickPaiId() || undefined;
        const criada = await firstValueFrom(this.catServicoRepo.criar({ nome, ...(paiId ? { categoriaPaiId: paiId } : {}) }));
        this.servicoCriado.emit(criada);
        if (paiId) {
          this.form.update(f => ({
            ...f,
            categoriasServicoBloco: [...f.categoriasServicoBloco, { categoriaServicoId: paiId, subcategoriasSelecionadas: [criada.id] }]
          }));
          this.atualizarCategoriasServico();
        } else {
          this.atualizarCategoriasServico();
          this.adicionarBlocoServico(criada.id);
        }
        this.clearError('servicos');
      } else {
        const criada = await firstValueFrom(this.pessoaRepo.criar({ nome, telefone: this.quickTelefone().trim(), tipo: 'Cliente' }));
        this.clienteCriado.emit(criada);
        let descricaoSet = false;
        this.form.update(f => {
          const shouldSetDescricao = this.devePuxarDescricaoCliente(f.descricao);
          if (shouldSetDescricao) descricaoSet = true;
          return {
            ...f,
            idCliente: criada.id,
            descricao: shouldSetDescricao ? nome : f.descricao
          };
        });
        this.clearError('idCliente');
        if (descricaoSet) {
          this.clearError('descricao');
        }
      }
      this.quickAdd.set(null);
    } catch (e) {
      this.quickError.set(mensagemErro(e, 'Erro ao salvar'));
    } finally {
      this.quickSalvando.set(false);
    }
  }

  private emptyForm(): LancamentoForm {
    return {
      descricao: '', valor: 0, data: '', idConta: '', idCategoria: '',
      categoriasServicoBloco: [], repete: false, dia: 1, diaUtil: false, dataFim: ''
    };
  }
}
