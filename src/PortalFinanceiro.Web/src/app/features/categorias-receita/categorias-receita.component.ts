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
  categoriaAlvo = signal<Categoria | null>(null);
  salvando = signal(false);
  movendo = signal(false);
  form: CategoriaRequest = { nome: '' };

  drawerSubsVisible = signal(false);
  buscaSub = signal('');
  nomeCategoriaDrawer = '';
  novaSubDrawer = '';
  subEmEdicaoId = signal<string | null>(null);
  nomeSubEdicao = '';
  destinoMover: Record<string, string> = {};

  private dragSub: DragSub | null = null;
  cardAlvoId = signal<string | null>(null);

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

  abrirModal() {
    this.form = { nome: '' };
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
  }

  async salvar() {
    if (!this.form.nome?.trim()) { this.notify.error('Informe o nome da categoria'); return; }
    this.salvando.set(true);
    try {
      await firstValueFrom(this.repo.criar({ nome: this.form.nome.trim() }));
      this.notify.success('Categoria criada');
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar categoria')); }
    finally { this.salvando.set(false); }
  }

  abrirDrawerSubs(categoria: Categoria) {
    this.categoriaAlvo.set(categoria);
    this.buscaSub.set('');
    this.nomeCategoriaDrawer = categoria.nome;
    this.novaSubDrawer = '';
    this.subEmEdicaoId.set(null);
    this.nomeSubEdicao = '';
    this.destinoMover = {};
    this.drawerSubsVisible.set(true);
  }

  fecharDrawerSubs() {
    this.drawerSubsVisible.set(false);
    this.buscaSub.set('');
    this.subEmEdicaoId.set(null);
  }

  destinosMoverDe(sub: Categoria) {
    return this.categoriasPai().filter(c => c.id !== sub.categoriaPaiId);
  }

  async salvarNomeCategoriaDrawer() {
    const foco = this.categoriaAlvo();
    const nome = this.nomeCategoriaDrawer?.trim();
    if (!foco || !nome) { this.notify.error('Informe o nome da categoria'); return; }
    if (nome === foco.nome) return;
    this.salvando.set(true);
    try {
      await firstValueFrom(this.repo.atualizar(foco.id, { nome }));
      this.notify.success('Categoria atualizada');
      await this.carregar();
      this.categoriaAlvo.set(this.items().find(c => c.id === foco.id) ?? null);
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar categoria')); }
    finally { this.salvando.set(false); }
  }

  async criarSubDrawer() {
    const foco = this.categoriaAlvo();
    const nome = this.novaSubDrawer?.trim();
    if (!foco || !nome) { this.notify.error('Informe o nome da subcategoria'); return; }
    this.salvando.set(true);
    try {
      await firstValueFrom(this.repo.criar({ nome, categoriaPaiId: foco.id }));
      this.notify.success('Subcategoria criada');
      this.novaSubDrawer = '';
      await this.carregar();
      this.categoriaAlvo.set(this.items().find(c => c.id === foco.id) ?? null);
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao criar subcategoria')); }
    finally { this.salvando.set(false); }
  }

  iniciarEdicaoSub(sub: Categoria) {
    this.subEmEdicaoId.set(sub.id);
    this.nomeSubEdicao = sub.nome;
  }

  cancelarEdicaoSub() {
    this.subEmEdicaoId.set(null);
    this.nomeSubEdicao = '';
  }

  async salvarEdicaoSub(sub: Categoria) {
    const nome = this.nomeSubEdicao?.trim();
    if (!nome) { this.notify.error('Informe o nome da subcategoria'); return; }
    if (nome === sub.nome) { this.cancelarEdicaoSub(); return; }
    this.salvando.set(true);
    try {
      await firstValueFrom(this.repo.atualizar(sub.id, { nome, ...(sub.categoriaPaiId ? { categoriaPaiId: sub.categoriaPaiId } : {}) }));
      this.notify.success('Subcategoria atualizada');
      this.cancelarEdicaoSub();
      const focoId = this.categoriaAlvo()?.id;
      await this.carregar();
      if (focoId) this.categoriaAlvo.set(this.items().find(c => c.id === focoId) ?? null);
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar subcategoria')); }
    finally { this.salvando.set(false); }
  }

  async moverParaDestino(sub: Categoria) {
    const destinoId = this.destinoMover[sub.id];
    if (!destinoId || destinoId === sub.categoriaPaiId) return;
    this.destinoMover[sub.id] = '';
    await this.moverSub(sub.id, sub.nome, sub.categoriaPaiId!, destinoId);
  }

  iniciarArrasto(event: DragEvent, sub: Categoria) {
    if (!sub.categoriaPaiId) return;
    this.dragSub = { id: sub.id, nome: sub.nome, origemId: sub.categoriaPaiId };
    event.dataTransfer?.setData('text/plain', sub.id);
    if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move';
  }

  finalizarArrasto() {
    this.dragSub = null;
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
