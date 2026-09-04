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
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [FormsModule, ModalComponent, SectionHeaderComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './clientes.component.html',
  styleUrl: './clientes.component.scss'
})
export class ClientesComponent implements OnInit {
  private repo = inject(PessoaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  clientes = signal<Pessoa[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Pessoa | null>(null);
  salvando = signal(false);

  form: PessoaRequest = { nome: '', telefone: '', tipo: 'Cliente' };

  clientesPaginacao = useListPagination(this.clientes, { initialPageSize: 10 });

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar());
      this.clientes.set(data.filter(p => p.tipo === 'Cliente'));
    } catch { this.notify.error('Erro ao carregar clientes'); }
    finally { this.loading.set(false); }
  }

  abrirModal() {
    this.form = { nome: '', telefone: '', tipo: 'Cliente' };
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
        this.notify.success('Cliente atualizado');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Cliente criado');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar cliente')); }
    finally { this.salvando.set(false); }
  }

  async excluir(pessoa: Pessoa) {
    const ok = await this.confirmService.confirm('Excluir cliente', `Deseja excluir "${pessoa.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(pessoa.id));
      this.notify.success('Cliente excluído');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir cliente')); }
  }
}
