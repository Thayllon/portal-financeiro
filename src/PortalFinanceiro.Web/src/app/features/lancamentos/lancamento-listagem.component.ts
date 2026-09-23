import { Component, inject, signal, input, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { ReceitaRepository, DespesaRepository, LancamentoFiltros } from '../../core/repositories/lancamento.repository';
import { CategoriaReceitaRepository, CategoriaDespesaRepository, CategoriaServicoRepository } from '../../core/repositories/categoria.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { ParceriaRepository } from '../../core/repositories/parceria.repository';
import { ContratoRepository } from '../../core/repositories/contrato.repository';
import { AuthService } from '../../core/services/auth.service';
import { ReceitaRequest } from '../../core/models/receita.model';
import { STATUS_PENDENTE, STATUS_REALIZADO } from '../../core/models/status.model';
import { Categoria } from '../../core/models/categoria.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { Pessoa } from '../../core/models/pessoa.model';
import { Parceria } from '../../core/models/parceria.model';
import { Contrato } from '../../core/models/contrato.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { LancamentoModalComponent, LancamentoForm } from '../../shared/components/lancamento-modal.component';
import { FluxoSelectorModalComponent } from '../../shared/components/fluxo-selector-modal.component';
import { ValorMascaradoPipe } from '../../shared/pipes/valor-mascarado.pipe';
import { PrivacidadeToggleComponent } from '../../shared/components/privacidade-toggle.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

export type LancamentoTipo = 'receita' | 'despesa';

interface ServicoItem {
  id: string;
  categoriaServicoId: string;
  categoriaServico: string;
  subcategoriaServicoId?: string;
  subcategoriaServico: string;
}

interface LancamentoItem {
  id: string;
  descricao: string;
  valor: number;
  data: string;
  idConta: string;
  conta: string;
  idCategoria: string;
  categoria: string;
  idSubcategoria?: string;
  subcategoria: string;
  idParceiro?: string;
  parceiro?: string;
  idParceria?: string;
  parceria: string;
  parceriaValor?: number;
  parceriaPercentual?: number;
  idContrato?: string;
  contrato?: string;
  idCliente?: string;
  cliente: string;
  servicos?: ServicoItem[];
  status: number;
  ehRecorrente: boolean;
  ativo: boolean;
  dataCadastro: string;
}

@Component({
  selector: 'app-lancamento-listagem',
  standalone: true,
  imports: [DatePipe, FormsModule, MonthNavComponent, StatusBadgeComponent, LancamentoModalComponent, FluxoSelectorModalComponent, ValorMascaradoPipe, PrivacidadeToggleComponent, CustomSelectComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './lancamento-listagem.component.html',
  styleUrl: './lancamento-listagem.component.scss'
})
export class LancamentoListagemComponent implements OnInit {
  tipo = input<LancamentoTipo>('receita');

  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private receitaRepo = inject(ReceitaRepository);
  private despesaRepo = inject(DespesaRepository);
  private catReceitaRepo = inject(CategoriaReceitaRepository);
  private catDespesaRepo = inject(CategoriaDespesaRepository);
  private contaRepo = inject(ContaBancariaRepository);
  private pessoaRepo = inject(PessoaRepository);
  private parceriaRepo = inject(ParceriaRepository);
  private contratoRepo = inject(ContratoRepository);
  private catServicoRepo = inject(CategoriaServicoRepository);
  private auth = inject(AuthService);

  private ehReceita = computed(() => this.tipo() === 'receita');

  config = computed(() => {
    const receita = this.ehReceita();
    return {
      titulo: receita ? 'Receitas' : 'Despesas',
      icone: receita ? 'trending-up' : 'trending-down',
      singular: receita ? 'receita' : 'despesa',
      rotuloRealizado: receita ? 'Recebido' : 'Pago',
      acaoRealizar: receita ? 'Receber' : 'Pagar',
      statusRealizadoLabel: receita ? 'Recebida' : 'Paga',
      hasParceiro: receita
    };
  });

  hasParceiro = computed(() => this.config().hasParceiro);

  items = signal<LancamentoItem[]>([]);
  categorias = signal<Categoria[]>([]);
  contas = signal<ContaBancaria[]>([]);
  parceiros = signal<Pessoa[]>([]);
  parcerias = signal<Parceria[]>([]);
  contratos = signal<Contrato[]>([]);
  clientes = signal<Pessoa[]>([]);
  categoriasServico = signal<Categoria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<LancamentoItem | null>(null);
  salvando = signal(false);

  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());
  filtroConta = '';
  filtroStatus = '';
  filtroCategoria = '';
  busca = '';
  private _buscaTimer: ReturnType<typeof setTimeout> | null = null;

  statusOptions = computed<SelectOption[]>(() => [
    { value: '1', label: 'Pendentes' },
    { value: '2', label: this.ehReceita() ? 'Recebidas' : 'Pagas' },
  ]);

  ordenacao = signal<{ coluna: 'valor' | 'data'; direcao: 'asc' | 'desc' } | null>(null);

  itensOrdenados = computed(() => {
    const ord = this.ordenacao();
    if (!ord) return this.items();
    const fator = ord.direcao === 'asc' ? 1 : -1;
    return [...this.items()].sort((a, b) =>
      ord.coluna === 'valor' ? (a.valor - b.valor) * fator : a.data.localeCompare(b.data) * fator
    );
  });

  alternarOrdenacao(coluna: 'valor' | 'data') {
    const atual = this.ordenacao();
    if (atual?.coluna === coluna) {
      this.ordenacao.set({ coluna, direcao: atual.direcao === 'asc' ? 'desc' : 'asc' });
    } else {
      this.ordenacao.set({ coluna, direcao: 'asc' });
    }
  }

  readonly statusRealizado = STATUS_REALIZADO;

  fluxoAdicional = computed(() => this.ehReceita() ? this.auth.temFluxoAdicionalReceita() : this.auth.temFluxoAdicionalDespesa());
  fluxoSelectorVisible = signal(false);
  fluxoContratoEscolhido = signal<boolean | null>(null);
  fluxoEfetivo = computed(() => {
    const escolha = this.fluxoContratoEscolhido();
    if (escolha !== null) return escolha;
    const editando = this.editando();
    if (editando) return this.inferirFluxoContrato(editando);
    return this.fluxoAdicional();
  });
  contasOptions = computed(() => this.contas().map(c => ({ value: c.id, label: `${c.nome} (${c.banco})` })));
  categoriasOptions = computed(() => this.categorias().map(c => ({ value: c.id, label: c.nome })));
  parceirosOptions = computed(() => this.parceiros().map(p => ({ value: p.id, label: p.nome })));
  clientesOptions = computed(() => this.clientes().map(c => ({ value: c.id, label: c.nome })));
  categoriasServicoOptions = computed(() => this.categoriasServico().map(c => ({ value: c.id, label: c.nome })));

  pagination = useListPagination(this.itensOrdenados, { initialPageSize: 10 });

  async ngOnInit() {
    const cargas: Promise<void>[] = [
      this.carregarCategorias(),
      this.carregarContas(),
      this.carregarParcerias(),
      this.carregarClientes(),
      this.carregarCategoriasServico()
    ];
    if (this.hasParceiro()) cargas.push(this.carregarParceiros(), this.carregarContratos());
    await Promise.all(cargas);
    await this.carregar();
  }

  async carregar() {
    this.loading.set(true);
    try {
      const filtros: LancamentoFiltros = {
        mes: this.mes(),
        ano: this.ano(),
        ...(this.filtroConta ? { idConta: this.filtroConta } : {}),
        ...(this.filtroStatus ? { status: Number(this.filtroStatus) } : {}),
        ...(this.filtroCategoria ? { idCategoria: this.filtroCategoria } : {}),
        ...(this.busca ? { busca: this.busca } : {})
      };
      const itens = this.ehReceita()
        ? await firstValueFrom(this.receitaRepo.listar(filtros))
        : await firstValueFrom(this.despesaRepo.listar(filtros));
      this.items.set(itens);
    } catch { this.notify.error(`Erro ao carregar ${this.config().singular}${this.ehReceita() ? 's' : 's'}`); }
    finally { this.loading.set(false); }
  }

  async carregarCategorias() {
    try {
      const itens = this.ehReceita()
        ? await firstValueFrom(this.catReceitaRepo.listar())
        : await firstValueFrom(this.catDespesaRepo.listar());
      this.categorias.set(itens);
    } catch {}
  }

  async carregarContas() {
    try { this.contas.set(await firstValueFrom(this.contaRepo.listar())); } catch {}
  }

  async carregarParceiros() {
    try {
      const todas = await firstValueFrom(this.pessoaRepo.listar());
      this.parceiros.set(todas.filter(p => p.tipo === 'Parceiro'));
    } catch {}
  }

  async carregarParcerias() {
    try { this.parcerias.set(await firstValueFrom(this.parceriaRepo.listar(true))); } catch {}
  }

  async carregarContratos() {
    try { this.contratos.set(await firstValueFrom(this.contratoRepo.listar(true))); } catch {}
  }

  async carregarClientes() {
    try {
      const todas = await firstValueFrom(this.pessoaRepo.listar());
      this.clientes.set(todas.filter(p => p.tipo === 'Cliente'));
    } catch {}
  }

  async carregarCategoriasServico() {
    try { this.categoriasServico.set(await firstValueFrom(this.catServicoRepo.listar())); } catch {}
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

  abrirModal(item?: LancamentoItem) {
    if (item) {
      this.fluxoContratoEscolhido.set(this.inferirFluxoContrato(item));
      this.editando.set(item);
      this.modalVisible.set(true);
      return;
    }
    if (this.fluxoAdicional()) {
      this.fluxoContratoEscolhido.set(null);
      this.editando.set(null);
      this.fluxoSelectorVisible.set(true);
      return;
    }
    this.fluxoContratoEscolhido.set(false);
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  escolherFluxo(contrato: boolean) {
    this.fluxoSelectorVisible.set(false);
    this.fluxoContratoEscolhido.set(contrato);
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  fecharSeletor() {
    this.fluxoSelectorVisible.set(false);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
    this.fluxoContratoEscolhido.set(null);
  }

  private inferirFluxoContrato(item: LancamentoItem): boolean {
    return !!(item.idCliente || (item.servicos && item.servicos.length > 0) || item.idParceria || item.idContrato);
  }

  onCategoriaCriada(categoria: Categoria) {
    this.categorias.update(list => [...list, categoria]);
  }

  onServicoCriado(categoria: Categoria) {
    this.categoriasServico.update(list => [...list, categoria]);
  }

  onClienteCriado(cliente: Pessoa) {
    this.clientes.update(list => [...list, cliente]);
  }

  copiar(item: LancamentoItem) {
    const copia: LancamentoItem = {
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
      ...(this.hasParceiro() ? { idParceiro: item.idParceiro, parceiro: item.parceiro } : {}),
      idParceria: item.idParceria,
      parceria: item.parceria,
      parceriaValor: item.parceriaValor,
      parceriaPercentual: item.parceriaPercentual,
      idContrato: item.idContrato,
      contrato: item.contrato,
      idCliente: item.idCliente,
      cliente: item.cliente,
      servicos: item.servicos?.map(s => ({
        id: s.id,
        categoriaServicoId: s.categoriaServicoId,
        categoriaServico: s.categoriaServico,
        subcategoriaServicoId: s.subcategoriaServicoId,
        subcategoriaServico: s.subcategoriaServico
      })) ?? [],
      status: STATUS_PENDENTE,
      ehRecorrente: false,
      ativo: true,
      dataCadastro: new Date().toISOString(),
    };
    this.fluxoContratoEscolhido.set(this.inferirFluxoContrato(copia));
    this.editando.set(copia);
    this.modalVisible.set(true);
  }

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
        idParceria: data.idParceria || undefined,
        idContrato: this.ehReceita() ? data.idContrato || undefined : undefined,
        servicos: data.servicos?.map(s => ({
          categoriaServicoId: s.categoriaServicoId,
          subcategoriaServicoId: s.subcategoriaServicoId
        })) || undefined,
        idCliente: data.idCliente || undefined,
        repete: data.repete,
        dia: data.repete ? data.dia : undefined,
        diaUtil: data.repete ? data.diaUtil : undefined,
        dataFim: data.repete ? data.dataFim + 'T00:00:00' : undefined
      };
      const rotulo = this.config().singular;
      if (this.editando()?.id) {
        if (this.ehReceita()) {
          await firstValueFrom(this.receitaRepo.atualizar(this.editando()!.id, request));
        } else {
          await firstValueFrom(this.despesaRepo.atualizar(this.editando()!.id, request));
        }
        this.notify.success(`${rotulo.charAt(0).toUpperCase() + rotulo.slice(1)} atualizada`);
      } else {
        if (this.ehReceita()) {
          await firstValueFrom(this.receitaRepo.criar(request));
        } else {
          await firstValueFrom(this.despesaRepo.criar(request));
        }
        this.notify.success(data.repete ? `${rotulo.charAt(0).toUpperCase() + rotulo.slice(1)} recorrente criada` : `${rotulo.charAt(0).toUpperCase() + rotulo.slice(1)} criada`);
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, `Erro ao salvar ${this.config().singular}`)); }
    finally { this.salvando.set(false); }
  }

  async realizar(item: LancamentoItem) {
    const data = { data: new Date().toISOString().split('T')[0] };
    const acao = this.config().acaoRealizar;
    try {
      if (this.ehReceita()) {
        await firstValueFrom(this.receitaRepo.receber(item.id, data));
      } else {
        await firstValueFrom(this.despesaRepo.pagar(item.id, data));
      }
      this.notify.success(`${this.config().singular} ${acao === 'Receber' ? 'recebida' : 'paga'}`);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, `Erro ao ${acao.toLowerCase()}`)); }
  }

  async estornar(item: LancamentoItem) {
    try {
      if (this.ehReceita()) {
        await firstValueFrom(this.receitaRepo.estornar(item.id));
      } else {
        await firstValueFrom(this.despesaRepo.estornar(item.id));
      }
      this.notify.success(`${this.config().singular} estornada`);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao estornar')); }
  }

  async excluir(item: LancamentoItem) {
    const rotulo = this.config().singular;
    const ok = await this.confirmService.confirm(`Excluir ${rotulo}`, `Deseja excluir "${item.descricao}"?`);
    if (!ok) return;
    try {
      if (this.ehReceita()) {
        await firstValueFrom(this.receitaRepo.excluir(item.id));
      } else {
        await firstValueFrom(this.despesaRepo.excluir(item.id));
      }
      this.notify.success(`${rotulo.charAt(0).toUpperCase() + rotulo.slice(1)} excluída`);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, `Erro ao excluir ${rotulo}`)); }
  }

  total = computed(() => this.items().reduce((s, l) => s + l.valor, 0));

  rotuloCategoria(l: LancamentoItem): string {
    if (this.ehReceita() && l.servicos?.length) {
      return l.servicos
        .map(s => s.subcategoriaServico ? `${s.categoriaServico} → ${s.subcategoriaServico}` : s.categoriaServico)
        .join(', ');
    }
    return l.subcategoria ? `${l.categoria} → ${l.subcategoria}` : l.categoria;
  }
  totalRealizado = computed(() => this.items().filter(l => l.status === STATUS_REALIZADO).reduce((s, l) => s + l.valor, 0));
  totalPendente = computed(() => this.total() - this.totalRealizado());
  totalParceria = computed(() => this.items().filter(l => !!l.idParceria).reduce((s, l) => s + l.valor, 0));
  repasseParceiro = computed(() => this.items()
    .filter(l => !!l.idParceria && l.status === STATUS_REALIZADO)
    .reduce((s, l) => s + l.valor * (l.parceriaPercentual ?? 0) / 100, 0));
  receitaLiquida = computed(() => this.totalRealizado() - this.repasseParceiro());
}