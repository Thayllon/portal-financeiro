import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { DespesaRepository, LancamentoFiltros as DespesaFiltros } from '../../core/repositories/lancamento.repository';
import { CategoriaDespesaRepository } from '../../core/repositories/categoria.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { Despesa, DespesaRequest } from '../../core/models/despesa.model';
import { STATUS_PENDENTE, STATUS_REALIZADO } from '../../core/models/status.model';
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
import { mensagemErro } from '../../shared/utils/api-error.util';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-despesas',
  standalone: true,
  imports: [DatePipe, FormsModule, MonthNavComponent, StatusBadgeComponent, LancamentoModalComponent, CurrencyBRLPipe, CustomSelectComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './despesas.component.html',
  styleUrl: './despesas.component.scss'
})
export class DespesasComponent implements OnInit {
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
  private _buscaTimer: ReturnType<typeof setTimeout> | null = null;

  statusOptions: SelectOption[] = [
    { value: '1', label: 'Pendentes' },
    { value: '2', label: 'Pagas' },
  ];

  readonly statusRealizado = STATUS_REALIZADO;

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
      const filtros: DespesaFiltros = {
        mes: this.mes(),
        ano: this.ano(),
        ...(this.filtroConta ? { idConta: this.filtroConta } : {}),
        ...(this.filtroStatus ? { status: Number(this.filtroStatus) } : {}),
        ...(this.filtroCategoria ? { idCategoria: this.filtroCategoria } : {}),
        ...(this.busca ? { busca: this.busca } : {})
      };
      this.items.set(await firstValueFrom(this.repo.listar(filtros)));
    } catch { this.notify.error('Erro ao carregar despesas'); }
    finally { this.loading.set(false); }
  }

  async carregarCategorias() {
    try { this.categorias.set(await firstValueFrom(this.catRepo.listar())); } catch {}
  }

  async carregarContas() {
    try { this.contas.set(await firstValueFrom(this.contaRepo.listar())); } catch {}
  }

  onBuscaChange() {
    if (this._buscaTimer) clearTimeout(this._buscaTimer);
    this._buscaTimer = setTimeout(() => this.carregar(), 400);
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

  copiar(item: Despesa) {
    const copia: Despesa = {
      id: '',
      descricao: item.descricao,
      valor: item.valor,
      data: item.data,
      idConta: item.idConta,
      conta: item.conta,
      idCategoria: item.idCategoria,
      categoria: item.categoria,
      idSubcategoria: item.idSubcategoria,
      subcategoria: item.subcategoria,
      status: STATUS_PENDENTE,
      ehRecorrente: false,
      ativo: true,
      dataCadastro: new Date().toISOString(),
    };
    this.editando.set(copia);
    this.modalVisible.set(true);
  }

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
        await firstValueFrom(this.repo.criar(request));
        this.notify.success(data.repete ? 'Despesa recorrente criada' : 'Despesa criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar despesa')); }
    finally { this.salvando.set(false); }
  }

  async pagar(item: Despesa) {
    try {
      await firstValueFrom(this.repo.pagar(item.id, { data: new Date().toISOString().split('T')[0] }));
      this.notify.success('Despesa paga');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao pagar')); }
  }

  async estornar(item: Despesa) {
    try {
      await firstValueFrom(this.repo.estornar(item.id));
      this.notify.success('Despesa estornada');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao estornar')); }
  }

  async excluir(item: Despesa) {
    const ok = await this.confirmService.confirm('Excluir despesa', `Deseja excluir "${item.descricao}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Despesa excluída');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir despesa')); }
  }

  total = computed(() => this.items().reduce((s, l) => s + l.valor, 0));
  totalPago = computed(() => this.items().filter(l => l.status === STATUS_REALIZADO).reduce((s, l) => s + l.valor, 0));
  totalPendente = computed(() => this.total() - this.totalPago());
}
