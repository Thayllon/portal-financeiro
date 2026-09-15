import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ParceriaRepository } from '../../core/repositories/parceria.repository';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { Parceria, ParceriaRequest } from '../../core/models/parceria.model';
import { Pessoa } from '../../core/models/pessoa.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { CurrencyInputDirective } from '../../shared/directives/currency-input.directive';
import { CurrencyBRLPipe } from '../../shared/pipes/currency-brl.pipe';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-parcerias',
  standalone: true,
  imports: [FormsModule, ModalComponent, CustomSelectComponent, CurrencyInputDirective, CurrencyBRLPipe, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './parcerias.component.html',
  styleUrl: './parcerias.component.scss'
})
export class ParceriasComponent implements OnInit {
  private repo = inject(ParceriaRepository);
  private pessoaRepo = inject(PessoaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  parcerias = signal<Parceria[]>([]);
  parceiros = signal<Pessoa[]>([]);
  clientes = signal<Pessoa[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Parceria | null>(null);
  salvando = signal(false);

  form: ParceriaRequest = { idParceiro: '', idCliente: '', valor: 0 };

  parceirosOptions = computed<SelectOption[]>(() => this.parceiros().map(p => ({ value: p.id, label: p.nome })));
  clientesOptions = computed<SelectOption[]>(() => this.clientes().map(p => ({ value: p.id, label: p.nome })));

  paginacao = useListPagination(this.parcerias, { initialPageSize: 10 });

  ngOnInit() { this.carregarPessoas(); this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar());
      this.parcerias.set(data);
    } catch { this.notify.error('Erro ao carregar parcerias'); }
    finally { this.loading.set(false); }
  }

  async carregarPessoas() {
    try {
      const todas = await firstValueFrom(this.pessoaRepo.listar());
      this.parceiros.set(todas.filter(p => p.tipo === 'Parceiro'));
      this.clientes.set(todas.filter(p => p.tipo === 'Cliente'));
    } catch {}
  }

  abrirModal() {
    this.form = { idParceiro: '', idCliente: '', valor: 0 };
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  editar(item: Parceria) {
    this.form = { idParceiro: item.idParceiro, idCliente: item.idCliente, valor: item.valor };
    this.editando.set(item);
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.idParceiro) { this.notify.error('Selecione o parceiro'); return; }
    if (!this.form.idCliente) { this.notify.error('Selecione o cliente'); return; }
    if (!this.form.valor || this.form.valor <= 0) { this.notify.error('Informe um valor válido'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Parceria atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Parceria criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar parceria')); }
    finally { this.salvando.set(false); }
  }

  async excluir(item: Parceria) {
    const ok = await this.confirmService.confirm('Excluir parceria', `Deseja excluir a parceria "${item.parceiro} - ${item.cliente}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Parceria excluída');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir parceria')); }
  }
}
