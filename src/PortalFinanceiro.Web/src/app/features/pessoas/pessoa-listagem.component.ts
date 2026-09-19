import { Component, inject, signal, input, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { Pessoa, PessoaRequest } from '../../core/models/pessoa.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

export type TipoPessoaListagem = 'Cliente' | 'Parceiro';

@Component({
  selector: 'app-pessoa-listagem',
  standalone: true,
  imports: [FormsModule, ModalComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './pessoa-listagem.component.html',
  styleUrl: './pessoa-listagem.component.scss'
})
export class PessoaListagemComponent implements OnInit {
  tipo = input<TipoPessoaListagem>('Cliente');

  private repo = inject(PessoaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  rotulo = computed(() => this.tipo() === 'Cliente' ? 'Cliente' : 'Parceiro');
  rotuloPlural = computed(() => this.tipo() === 'Cliente' ? 'clientes' : 'parceiros');
  icone = computed(() => this.tipo() === 'Cliente' ? 'users' : 'handshake');
  placeholderNome = computed(() => this.tipo() === 'Cliente' ? 'Ex: João da Silva' : 'Ex: Empresa XYZ');

  itens = signal<Pessoa[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Pessoa | null>(null);
  salvando = signal(false);

  form: PessoaRequest = { nome: '', telefone: '', tipo: 'Cliente' };

  paginacao = useListPagination(this.itens, { initialPageSize: 10 });

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar());
      this.itens.set(data.filter(p => p.tipo === this.tipo()));
    } catch { this.notify.error(`Erro ao carregar ${this.rotuloPlural()}`); }
    finally { this.loading.set(false); }
  }

  abrirModal() {
    this.form = { nome: '', telefone: '', tipo: this.tipo() };
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  editar(item: Pessoa) {
    this.form = { nome: item.nome, telefone: item.telefone ?? '', tipo: item.tipo };
    this.editando.set(item);
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome) { this.notify.error('Informe o nome'); return; }
    this.salvando.set(true);
    try {
      const rotulo = this.rotulo();
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success(`${rotulo} atualizado`);
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success(`${rotulo} criado`);
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, `Erro ao salvar ${this.rotuloPlural()}`)); }
    finally { this.salvando.set(false); }
  }

  async excluir(pessoa: Pessoa) {
    const rotulo = this.rotulo().toLowerCase();
    const ok = await this.confirmService.confirm(`Excluir ${rotulo}`, `Deseja excluir "${pessoa.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(pessoa.id));
      this.notify.success(`${this.rotulo()} excluído`);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, `Erro ao excluir ${this.rotuloPlural()}`)); }
  }
}