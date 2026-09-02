import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { Pessoa, PessoaRequest } from '../../core/models/pessoa.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { SectionHeaderComponent } from '../../shared/components/section-header.component';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-pessoas',
  standalone: true,
  imports: [FormsModule, ModalComponent, SectionHeaderComponent, ListPaginationComponent, CustomSelectComponent, LucideDynamicIcon],
  templateUrl: './pessoas.component.html',
  styleUrl: './pessoas.component.scss'
})
export class PessoasComponent implements OnInit {
  private repo = inject(PessoaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  pessoas = signal<Pessoa[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Pessoa | null>(null);
  salvando = signal(false);

  form: PessoaRequest = { nome: '', telefone: '', tipo: 'Cliente' };

  tipoOptions: SelectOption[] = [
    { value: 'Cliente', label: 'Cliente' },
    { value: 'Parceiro', label: 'Parceiro' },
  ];

  clientes = computed(() => this.pessoas().filter(p => p.tipo === 'Cliente'));
  parceiros = computed(() => this.pessoas().filter(p => p.tipo === 'Parceiro'));

  clientesPagination = useListPagination(this.clientes, { initialPageSize: 10 });
  parceirosPagination = useListPagination(this.parceiros, { initialPageSize: 10 });

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar());
      this.pessoas.set(data);
    } catch { this.notify.error('Erro ao carregar pessoas'); }
    finally { this.loading.set(false); }
  }

  abrirModal(tipo: string = 'Cliente') {
    this.form = { nome: '', telefone: '', tipo };
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
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Pessoa atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Pessoa criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar pessoa')); }
    finally { this.salvando.set(false); }
  }

  async excluir(pessoa: Pessoa) {
    const ok = await this.confirmService.confirm('Excluir pessoa', `Deseja excluir "${pessoa.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(pessoa.id));
      this.notify.success('Pessoa excluída');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir pessoa')); }
  }
}