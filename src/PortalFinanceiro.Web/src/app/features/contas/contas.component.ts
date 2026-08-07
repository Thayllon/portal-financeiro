import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { ContaBancaria, ContaBancariaRequest } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-contas',
  standalone: true,
  imports: [FormsModule, ModalComponent, StatusBadgeComponent, CustomSelectComponent, LucideDynamicIcon],
  templateUrl: './contas.component.html',
  styleUrl: './contas.component.scss'
})
export class ContasComponent implements OnInit {
  private repo = inject(ContaBancariaRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  contas = signal<ContaBancaria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<ContaBancaria | null>(null);
  salvando = signal(false);

  form: ContaBancariaRequest = { nome: '', banco: '', tipo: 'Pf' };

  tipoOptions: SelectOption[] = [
    { value: 'Pf', label: 'Pessoa Física' },
    { value: 'Pj', label: 'Pessoa Jurídica' },
  ];

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar());
      this.contas.set(data);
    } catch { this.notify.error('Erro ao carregar contas'); }
    finally { this.loading.set(false); }
  }

  abrirModal(item?: ContaBancaria) {
    if (item) {
      this.form = { nome: item.nome, banco: item.banco, tipo: item.tipo };
      this.editando.set(item);
    } else {
      this.form = { nome: '', banco: '', tipo: 'Pf' };
      this.editando.set(null);
    }
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome || !this.form.banco) { this.notify.error('Preencha todos os campos'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Conta atualizada');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Conta criada');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar conta')); }
    finally { this.salvando.set(false); }
  }

  async excluir(conta: ContaBancaria) {
    const ok = await this.confirmService.confirm('Excluir conta', `Deseja excluir "${conta.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(conta.id));
      this.notify.success('Conta excluída');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir conta')); }
  }
}
