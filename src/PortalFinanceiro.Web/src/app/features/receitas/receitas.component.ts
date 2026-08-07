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
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-receitas',
  standalone: true,
  imports: [DatePipe, FormsModule, MonthNavComponent, StatusBadgeComponent, LancamentoModalComponent, CurrencyBRLPipe, CustomSelectComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './receitas.component.html',
  styleUrl: './receitas.component.scss'
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
  private _buscaTimer: ReturnType<typeof setTimeout> | null = null;

  statusOptions: SelectOption[] = [
    { value: '1', label: 'Pendentes' },
    { value: '2', label: 'Recebidas' },
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
        mes: this.mes(),
        ano: this.ano(),
        ...(this.filtroConta ? { idConta: this.filtroConta } : {}),
        ...(this.filtroStatus ? { status: Number(this.filtroStatus) } : {}),
        ...(this.filtroCategoria ? { idCategoria: this.filtroCategoria } : {}),
        ...(this.busca ? { busca: this.busca } : {})
      };
      this.items.set(await firstValueFrom(this.repo.listar(filtros)));
    } catch { this.notify.error('Erro ao carregar receitas'); }
    finally { this.loading.set(false); }
  }

  async carregarCategorias() {
    try { this.categorias.set(await firstValueFrom(this.catRepo.listar(this.auth.user()!.usuarioId))); } catch {}
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
        await firstValueFrom(this.repo.criar(request));
        this.notify.success(data.repete ? 'Receita recorrente criada' : 'Receita criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar receita')); }
    finally { this.salvando.set(false); }
  }

  async receber(item: Receita) {
    try {
      await firstValueFrom(this.repo.receber(item.id, { data: new Date().toISOString().split('T')[0] }));
      this.notify.success('Receita recebida');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao receber')); }
  }

  async estornar(item: Receita) {
    try {
      await firstValueFrom(this.repo.estornar(item.id));
      this.notify.success('Receita estornada');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao estornar')); }
  }

  async excluir(item: Receita) {
    const ok = await this.confirmService.confirm('Excluir receita', `Deseja excluir "${item.descricao}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Receita excluída');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir receita')); }
  }

  total = computed(() => this.items().reduce((s, l) => s + l.valor, 0));
  totalRecebido = computed(() => this.items().filter(l => l.status === 'Realizado').reduce((s, l) => s + l.valor, 0));
  totalPendente = computed(() => this.total() - this.totalRecebido());
}
