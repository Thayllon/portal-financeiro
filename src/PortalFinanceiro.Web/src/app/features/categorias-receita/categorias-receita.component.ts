import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CategoriaReceitaRepository, CategoriaDespesaRepository, CategoriaServicoRepository } from '../../core/repositories/categoria.repository';
import { Categoria, CategoriaRequest } from '../../core/models/categoria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { SideDrawerComponent } from '../../shared/components/side-drawer.component';
import { TabsComponent, Tab } from '../../shared/components/tabs.component';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

export const LIMITE_SUBS_VISIVEIS = 5;

type ModalModo = 'nova-categoria' | 'nova-sub' | 'editar';

interface DragSub {
  id: string;
  nome: string;
  origemId: string;
}

@Component({
  selector: 'app-categorias',
  standalone: true,
  imports: [FormsModule, ModalComponent, SideDrawerComponent, TabsComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './categorias-receita.component.html',
  styleUrl: './categorias-receita.component.scss'
})
export class CategoriasComponent implements OnInit {
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private repoReceita = inject(CategoriaReceitaRepository);
  private repoDespesa = inject(CategoriaDespesaRepository);
  private repoServico = inject(CategoriaServicoRepository);

  tabs: Tab[] = [
    { id: 'receita', label: 'Receita' },
    { id: 'despesa', label: 'Despesa' },
    { id: 'servicos', label: 'Serviços' }
  ];
  tabAtiva = signal('receita');
  items = signal<Categoria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  modalModo = signal<ModalModo>('nova-categoria');
  categoriaAlvo = signal<Categoria | null>(null);
  editando = signal<Categoria | null>(null);
  salvando = signal(false);
  movendo = signal(false);
  form: CategoriaRequest = { nome: '', categoriaPaiId: undefined };

  drawerSubsVisible = signal(false);
  buscaSub = signal('');

  private dragSub: DragSub | null = null;
  cardAlvoId = signal<string | null>(null);
  arrastandoId = signal<string | null>(null);

  ngOnInit() { this.carregar(); }

  private get repo() {
    if (this.tabAtiva() === 'receita') return this.repoReceita;
    if (this.tabAtiva() === 'despesa') return this.repoDespesa;
    return this.repoServico;
  }

  trocarAba(tab: string) {
    this.tabAtiva.set(tab);
    this.fecharDrawerSubs();
    this.carregar();
  }

  async carregar() {
    this.loading.set(true);
    this.items.set([]);
    try {
      this.items.set(await firstValueFrom(this.repo.listar()));
    } catch { this.notify.error('Erro ao carregar categorias'); }
    finally { this.loading.set(false); }
  }

  categoriasPai() { return this.items().filter(c => !c.categoriaPaiId); }
  subcategoriasDe(paiId: string) { return this.items().filter(c => c.categoriaPaiId === paiId); }
  subsVisiveisDe(paiId: string) { return this.subcategoriasDe(paiId).slice(0, LIMITE_SUBS_VISIVEIS); }
  subsRestantesDe(paiId: string) { return Math.max(0, this.subcategoriasDe(paiId).length - LIMITE_SUBS_VISIVEIS); }

  paisSignal = computed(() => this.categoriasPai());
  pagination = useListPagination(this.paisSignal, { initialPageSize: 10 });
  paginatedPais = computed(() => this.pagination.paginatedItems());

  subsFiltradasDrawer = computed(() => {
    const foco = this.categoriaAlvo();
    if (!foco) return [];
    const busca = this.buscaSub().trim().toLowerCase();
    const todas = this.subcategoriasDe(foco.id);
    if (!busca) return todas;
    return todas.filter(s => s.nome.toLowerCase().includes(busca));
  });

  tituloModal(): string {
    const aba = this.tabAtiva();
    if (this.modalModo() === 'nova-sub') return `Nova subcategoria em ${this.categoriaAlvo()?.nome ?? ''}`;
    if (this.modalModo() === 'editar') {
      const item = this.editando();
      const ehSub = !!item?.categoriaPaiId;
      return ehSub ? 'Renomear subcategoria' : `Renomear categoria de ${aba}`;
    }
    return `Nova categoria de ${aba}`;
  }

  subtituloModal(): string {
    if (this.modalModo() === 'nova-sub') return 'Ela será vinculada à categoria selecionada.';
    if (this.modalModo() === 'editar' && this.editando()?.categoriaPaiId) {
      const pai = this.items().find(c => c.id === this.editando()!.categoriaPaiId);
      return pai ? `Subcategoria de ${pai.nome}. Para trocar de categoria, arraste pelo ícone de mover.` : '';
    }
    return '';
  }

  abrirModal(item?: Categoria, paiId?: string) {
    if (item) {
      this.modalModo.set('editar');
      this.editando.set(item);
      this.categoriaAlvo.set(item.categoriaPaiId ? (this.items().find(c => c.id === item.categoriaPaiId) ?? null) : null);
      this.form = { nome: item.nome, categoriaPaiId: item.categoriaPaiId };
    } else if (paiId) {
      const pai = this.items().find(c => c.id === paiId) ?? null;
      this.modalModo.set('nova-sub');
      this.editando.set(null);
      this.categoriaAlvo.set(pai);
      this.form = { nome: '', categoriaPaiId: paiId };
    } else {
      this.modalModo.set('nova-categoria');
      this.editando.set(null);
      this.categoriaAlvo.set(null);
      this.form = { nome: '', categoriaPaiId: undefined };
    }
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
    this.categoriaAlvo.set(null);
  }

  async salvar() {
    if (!this.form.nome?.trim()) { this.notify.error('Informe o nome da categoria'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        const payload: CategoriaRequest = { nome: this.form.nome.trim(), ...(this.editando()!.categoriaPaiId ? { categoriaPaiId: this.editando()!.categoriaPaiId } : {}) };
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, payload));
        this.notify.success('Categoria atualizada');
      } else {
        const payload: CategoriaRequest = {
          nome: this.form.nome.trim(),
          ...(this.form.categoriaPaiId ? { categoriaPaiId: this.form.categoriaPaiId } : {})
        };
        await firstValueFrom(this.repo.criar(payload));
        this.notify.success(this.modalModo() === 'nova-sub' ? 'Subcategoria criada' : 'Categoria criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar categoria')); }
    finally { this.salvando.set(false); }
  }

  abrirDrawerSubs(categoria: Categoria) {
    this.categoriaAlvo.set(categoria);
    this.buscaSub.set('');
    this.drawerSubsVisible.set(true);
  }

  fecharDrawerSubs() {
    this.drawerSubsVisible.set(false);
    this.buscaSub.set('');
  }

  iniciarArrasto(event: DragEvent, sub: Categoria) {
    if (!sub.categoriaPaiId) return;
    this.dragSub = { id: sub.id, nome: sub.nome, origemId: sub.categoriaPaiId };
    this.arrastandoId.set(sub.id);
    event.dataTransfer?.setData('text/plain', sub.id);
    if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move';
  }

  finalizarArrasto() {
    this.dragSub = null;
    this.arrastandoId.set(null);
    this.cardAlvoId.set(null);
  }

  sobreCard(event: DragEvent, destinoId: string) {
    if (!this.dragSub || this.dragSub.origemId === destinoId) return;
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
    this.cardAlvoId.set(destinoId);
  }

  sairCard() {
    this.cardAlvoId.set(null);
  }

  async soltarNoCard(event: DragEvent, destino: Categoria) {
    event.preventDefault();
    const arrastada = this.dragSub;
    this.cardAlvoId.set(null);
    if (!arrastada || arrastada.origemId === destino.id) {
      this.dragSub = null;
      return;
    }
    this.dragSub = null;
    await this.moverSub(arrastada.id, arrastada.nome, arrastada.origemId, destino.id);
  }

  private nomeCategoria(id: string): string {
    return this.items().find(c => c.id === id)?.nome ?? '';
  }

  async moverSub(subId: string, subNome: string, origemId: string, destinoId: string) {
    const origemNome = this.nomeCategoria(origemId);
    const destinoNome = this.nomeCategoria(destinoId);
    const ok = await this.confirmService.confirm(
      'Mover subcategoria',
      `Mover "${subNome}" de "${origemNome}" para "${destinoNome}"?`
    );
    if (!ok) return;
    this.movendo.set(true);
    try {
      await firstValueFrom(this.repo.atualizar(subId, { nome: subNome, categoriaPaiId: destinoId }));
      this.notify.success(`"${subNome}" movida para "${destinoNome}"`);
      await this.carregar();
      const foco = this.categoriaAlvo();
      if (foco && (foco.id === origemId || foco.id === destinoId) && this.drawerSubsVisible()) {
        this.categoriaAlvo.set(this.items().find(c => c.id === foco.id) ?? null);
      }
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao mover subcategoria')); }
    finally { this.movendo.set(false); }
  }

  async excluir(item: Categoria) {
    const temSub = this.subcategoriasDe(item.id).length > 0;
    const msg = temSub
      ? `Deseja excluir "${item.nome}" e todas as suas subcategorias?`
      : `Deseja excluir "${item.nome}"?`;

    const ok = await this.confirmService.confirm('Excluir categoria', msg);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Categoria excluída');
      if (this.categoriaAlvo()?.id === item.id || this.categoriaAlvo()?.id === item.categoriaPaiId) {
        this.categoriaAlvo.set(this.items().find(c => c.id === this.categoriaAlvo()!.id) ?? null);
      }
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir categoria')); }
  }
}
