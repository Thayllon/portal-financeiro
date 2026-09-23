import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ContratoRepository } from '../../core/repositories/contrato.repository';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { CategoriaReceitaRepository } from '../../core/repositories/categoria.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { Contrato, ContratoRequest } from '../../core/models/contrato.model';
import { Pessoa } from '../../core/models/pessoa.model';
import { Categoria } from '../../core/models/categoria.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { CurrencyInputDirective } from '../../shared/directives/currency-input.directive';
import { ValorMascaradoPipe } from '../../shared/pipes/valor-mascarado.pipe';
import { PrivacidadeToggleComponent } from '../../shared/components/privacidade-toggle.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-contratos',
  standalone: true,
  imports: [FormsModule, ModalComponent, CustomSelectComponent, CurrencyInputDirective, ValorMascaradoPipe, PrivacidadeToggleComponent, StatusBadgeComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './contratos.component.html',
  styleUrl: './contratos.component.scss'
})
export class ContratosComponent implements OnInit {
  private repo = inject(ContratoRepository);
  private pessoaRepo = inject(PessoaRepository);
  private catRepo = inject(CategoriaReceitaRepository);
  private contaRepo = inject(ContaBancariaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private router = inject(Router);

  contratos = signal<Contrato[]>([]);
  clientes = signal<Pessoa[]>([]);
  categorias = signal<Categoria[]>([]);
  contas = signal<ContaBancaria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  modalRecorrenteVisible = signal(false);
  editando = signal<Contrato | null>(null);
  salvando = signal(false);
  filtroSituacao: boolean | undefined = undefined;

  contratoAberto = signal(false);
  recorrenteAberto = signal(false);

  form: ContratoRequest = { nome: '', idCliente: '', valor: 0 };
  previewMeses = signal(0);

  clientesOptions = computed<SelectOption[]>(() => this.clientes().map(p => ({ value: p.id, label: p.nome })));
  categoriasOptions = computed<SelectOption[]>(() => this.categorias().filter(c => !c.categoriaPaiId).map(c => ({ value: c.id, label: c.nome })));
  contasOptions = computed<SelectOption[]>(() => this.contas().map(c => ({ value: c.id, label: `${c.nome} (${c.banco})` })));

  getSubcategoriasOptions(): SelectOption[] {
    const pai = this.form.idCategoria;
    if (!pai) return [];
    return this.categorias().filter(c => c.categoriaPaiId === pai).map(c => ({ value: c.id, label: c.nome }));
  }

  temSubcategorias(): boolean {
    return this.getSubcategoriasOptions().length > 0;
  }
  situacaoOptions: SelectOption[] = [
    { value: 'todas', label: 'Todas' },
    { value: 'ativas', label: 'Ativas' },
    { value: 'encerradas', label: 'Encerradas' }
  ];

  contratosNormais = computed(() => this.contratos().filter(c => !c.ehRecorrente));
  contratosRecorrentes = computed(() => this.contratos().filter(c => c.ehRecorrente));

  paginacao = useListPagination(this.contratos, { initialPageSize: 10 });
  paginacaoContrato = useListPagination(this.contratosNormais, { initialPageSize: 10 });
  paginacaoRecorrente = useListPagination(this.contratosRecorrentes, { initialPageSize: 10 });

  ngOnInit() { this.carregarClientes(); this.carregarCategorias(); this.carregarContas(); this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar(this.filtroSituacao));
      this.contratos.set(data);
    } catch { this.notify.error('Erro ao carregar contratos'); }
    finally { this.loading.set(false); }
  }

  mudarSituacao(valor: string | number | null) {
    this.filtroSituacao = valor === 'ativas' ? true : valor === 'encerradas' ? false : undefined;
    this.carregar();
  }

  async carregarClientes() {
    try {
      const todas = await firstValueFrom(this.pessoaRepo.listar());
      this.clientes.set(todas.filter(p => p.tipo === 'Cliente'));
    } catch {}
  }

  async carregarCategorias() {
    try { this.categorias.set(await firstValueFrom(this.catRepo.listar())); } catch {}
  }

  async carregarContas() {
    try { this.contas.set(await firstValueFrom(this.contaRepo.listar())); } catch {}
  }

  onCategoriaChange(value: string) {
    this.form.idCategoria = value || undefined;
    this.form.idSubcategoria = undefined;
  }

  calcularPreview() {
    const f = this.form;
    const ini = f.dataInicio;
    const fim = f.dataFim;
    if (ini && fim) {
      const inicio = new Date(ini + 'T00:00:00');
      const final = new Date(fim + 'T00:00:00');
      if (final <= inicio) { this.previewMeses.set(0); return; }
      const meses = (final.getFullYear() - inicio.getFullYear()) * 12 + (final.getMonth() - inicio.getMonth());
      this.previewMeses.set(meses > 0 ? meses : 1);
    } else {
      this.previewMeses.set(0);
    }
  }

  onDiaUtilChange() {
    if (this.form.diaUtil && this.form.dia && this.form.dia > 5) {
      this.form.dia = 5;
    }
    this.calcularPreview();
  }

  abrirModal() {
    this.form = { nome: '', idCliente: '', valor: 0, ehRecorrente: false };
    this.editando.set(null);
    this.previewMeses.set(0);
    this.modalVisible.set(true);
  }

  abrirModalRecorrente() {
    const hoje = new Date().toISOString().split('T')[0];
    const umAno = new Date();
    umAno.setFullYear(umAno.getFullYear() + 1);
    const fim = umAno.toISOString().split('T')[0];
    this.form = {
      nome: '', idCliente: '', valor: 0, ehRecorrente: true,
      idCategoria: undefined, idSubcategoria: undefined, idConta: this.contas().find(c => c.ehPadrao)?.id ?? this.contas()[0]?.id ?? '',
      dia: 1, diaUtil: false, dataInicio: hoje, dataFim: fim
    };
    this.editando.set(null);
    this.previewMeses.set(0);
    this.calcularPreview();
    this.modalRecorrenteVisible.set(true);
  }

  editar(item: Contrato) {
    if (item.ehRecorrente) {
      this.form = { nome: item.nome, idCliente: item.idCliente, valor: item.valor, ehRecorrente: true };
      this.editando.set(item);
      this.modalRecorrenteVisible.set(true);
      return;
    }
    this.form = { nome: item.nome, idCliente: item.idCliente, valor: item.valor, ehRecorrente: false };
    this.editando.set(item);
    this.modalVisible.set(true);
  }

  verDetalhes(item: Contrato) {
    this.router.navigate(['/contratos', item.id]);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  fecharModalRecorrente() {
    this.modalRecorrenteVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    if (!this.form.idCliente) { this.notify.error('Selecione o cliente'); return; }
    if (!this.form.valor || this.form.valor <= 0) { this.notify.error('Informe um valor válido'); return; }
    if (!this.editando() && this.form.ehRecorrente) {
      if (!this.form.idCategoria) { this.notify.error('Selecione a categoria'); return; }
      if (!this.form.idConta) { this.notify.error('Selecione a conta'); return; }
      const diaNum = Number(this.form.dia);
      if (!diaNum || diaNum < 1 || diaNum > 31) { this.notify.error('Dia deve estar entre 1 e 31'); return; }
      if (this.form.diaUtil && diaNum > 5) { this.notify.error('Dia útil deve estar entre 1 e 5'); return; }
      if (!this.form.dataFim) { this.notify.error('Informe a data fim'); return; }
      if (this.form.dataInicio && this.form.dataFim <= this.form.dataInicio) { this.notify.error('Data fim deve ser posterior à data início'); return; }
    }
    if (this.editando()?.ehRecorrente) {
      this.form.ehRecorrente = false;
      this.form.idCategoria = undefined;
      this.form.idSubcategoria = undefined;
      this.form.idConta = undefined;
      this.form.dia = undefined;
      this.form.diaUtil = undefined;
      this.form.dataInicio = undefined;
      this.form.dataFim = undefined;
    }
    const payload: ContratoRequest = { ...this.form };
    if (payload.ehRecorrente) {
      payload.dia = Number(payload.dia);
      if (payload.dataInicio && !payload.dataInicio.includes('T')) payload.dataInicio = payload.dataInicio + 'T00:00:00';
      if (payload.dataFim && !payload.dataFim.includes('T')) payload.dataFim = payload.dataFim + 'T00:00:00';
      if (!payload.idSubcategoria) payload.idSubcategoria = undefined;
      if (!payload.idCategoria) payload.idCategoria = undefined;
      if (!payload.idConta) payload.idConta = undefined;
    }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, payload));
        this.notify.success('Contrato atualizado');
      } else {
        await firstValueFrom(this.repo.criar(payload));
        this.notify.success(payload.ehRecorrente ? 'Contrato recorrente criado' : 'Contrato criado');
      }
      this.fecharModal();
      this.fecharModalRecorrente();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar contrato')); }
    finally { this.salvando.set(false); }
  }

  async alternarSituacao(item: Contrato) {
    if (item.ativo && item.faltaReceber > 0) {
      this.notify.error('Só é possível encerrar contrato sem valores a receber');
      return;
    }
    try {
      if (item.ativo) {
        await firstValueFrom(this.repo.encerrar(item.id));
        this.notify.success('Contrato encerrado');
      } else {
        await firstValueFrom(this.repo.reativar(item.id));
        this.notify.success('Contrato reativado');
      }
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao alterar situação do contrato')); }
  }

  async excluir(item: Contrato) {
    const ok = await this.confirmService.confirm('Excluir contrato', `Deseja excluir o contrato "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Contrato excluído');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir contrato')); }
  }
}
