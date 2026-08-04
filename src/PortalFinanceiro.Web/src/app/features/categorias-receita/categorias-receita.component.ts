import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CategoriaReceitaRepository, CategoriaDespesaRepository } from '../../core/repositories/categoria.repository';
import { Categoria, CategoriaRequest } from '../../core/models/categoria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { TabsComponent, Tab } from '../../shared/components/tabs.component';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-categorias',
  standalone: true,
  imports: [FormsModule, ModalComponent, TabsComponent, LucideDynamicIcon],
  templateUrl: './categorias-receita.component.html',
  styleUrl: './categorias-receita.component.scss'
})
export class CategoriasComponent implements OnInit {
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private repoReceita = inject(CategoriaReceitaRepository);
  private repoDespesa = inject(CategoriaDespesaRepository);

  tabs: Tab[] = [
    { id: 'receita', label: 'Receita' },
    { id: 'despesa', label: 'Despesa' }
  ];
  tabAtiva = signal('receita');
  items = signal<Categoria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Categoria | null>(null);
  salvando = signal(false);
  form: CategoriaRequest = { nome: '', categoriaPaiId: undefined };

  ngOnInit() { this.carregar(); }

  private get repo() {
    return this.tabAtiva() === 'receita' ? this.repoReceita : this.repoDespesa;
  }

  trocarAba(tab: string) {
    this.tabAtiva.set(tab);
    this.carregar();
  }

  async carregar() {
    this.loading.set(true);
    try {
      this.items.set(await firstValueFrom(this.repo.listar()));
    } catch { this.notify.error('Erro ao carregar categorias'); }
    finally { this.loading.set(false); }
  }

  categoriasPai() { return this.items().filter(c => !c.categoriaPaiId); }
  subcategoriasDe(paiId: string) { return this.items().filter(c => c.categoriaPaiId === paiId); }

  abrirModal(item?: Categoria, paiId?: string) {
    this.form = {
      nome: item?.nome ?? '',
      categoriaPaiId: item ? item.categoriaPaiId : (paiId ?? undefined)
    };
    this.editando.set(item ?? null);
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome) { this.notify.error('Informe o nome da categoria'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Categoria atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Categoria criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch { this.notify.error('Erro ao salvar categoria'); }
    finally { this.salvando.set(false); }
  }

  async excluir(item: Categoria) {
    const ok = await this.confirmService.confirm('Excluir categoria', `Deseja excluir "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Categoria excluída');
      await this.carregar();
    } catch { this.notify.error('Erro ao excluir categoria'); }
  }
}
