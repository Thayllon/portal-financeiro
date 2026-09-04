import { Component, inject, signal, OnInit } from '@angular/core';
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
  selector: 'app-parceiros',
  standalone: true,
  imports: [FormsModule, ModalComponent, SectionHeaderComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './parceiros.component.html',
  styleUrl: './parceiros.component.scss'
})
export class ParceirosComponent implements OnInit {
  private repo = inject(PessoaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  parceiros = signal<Pessoa[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Pessoa | null>(null);
  salvando = signal(false);

  form: PessoaRequest = { nome: '', telefone: '', tipo: 'Parceiro' };

  parceirosPaginacao = useListPagination(this.parceiros, { initialPageSize: 10 });

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar());
      this.parceiros.set(data.filter(p => p.tipo === 'Parceiro'));
    } catch { this.notify.error('Erro ao carregar parceiros'); }
    finally { this.loading.set(false); }
  }

  abrirModal() {
    this.form = { nome: '', telefone: '', tipo: 'Parceiro' };
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
        this.notify.success('Parceiro atualizado');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Parceiro criado');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar parceiro')); }
    finally { this.salvando.set(false); }
  }

  async excluir(pessoa: Pessoa) {
    const ok = await this.confirmService.confirm('Excluir parceiro', `Deseja excluir "${pessoa.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(pessoa.id));
      this.notify.success('Parceiro excluído');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir parceiro')); }
  }
}
