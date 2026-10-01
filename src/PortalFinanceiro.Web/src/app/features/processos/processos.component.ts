import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ProcessoRepository } from '../../core/repositories/processo.repository';
import { Processo, ProcessoRequest } from '../../core/models/processo.model';
import { AuthService } from '../../core/services/auth.service';
import { NivelPermissao } from '../../core/models/permissao.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-processos',
  standalone: true,
  imports: [FormsModule, ModalComponent, CustomSelectComponent, StatusBadgeComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './processos.component.html',
  styleUrl: './processos.component.scss'
})
export class ProcessosComponent implements OnInit {
  private repo = inject(ProcessoRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private router = inject(Router);
  private auth = inject(AuthService);

  podeEscrever = computed(() => this.auth.temPermissao('processos', NivelPermissao.Escrita));

  processos = signal<Processo[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Processo | null>(null);
  salvando = signal(false);
  filtroSituacao: boolean | undefined = undefined;

  form: ProcessoRequest = { nome: '', descricao: '' };

  situacaoOptions: SelectOption[] = [
    { value: 'todas', label: 'Todas' },
    { value: 'ativas', label: 'Ativas' },
    { value: 'encerradas', label: 'Encerradas' }
  ];

  paginacao = useListPagination(this.processos, { initialPageSize: 10 });

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar(this.filtroSituacao));
      this.processos.set(data);
    } catch { this.notify.error('Erro ao carregar processos'); }
    finally { this.loading.set(false); }
  }

  mudarSituacao(valor: string | number | null) {
    this.filtroSituacao = valor === 'ativas' ? true : valor === 'encerradas' ? false : undefined;
    this.carregar();
  }

  abrirModal() {
    this.form = { nome: '', descricao: '' };
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  editar(item: Processo) {
    this.form = { nome: item.nome, descricao: item.descricao ?? '' };
    this.editando.set(item);
    this.modalVisible.set(true);
  }

  verDetalhes(item: Processo) {
    this.router.navigate(['/processos', item.id]);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    this.salvando.set(true);
    try {
      const payload: ProcessoRequest = { nome: this.form.nome.trim(), descricao: this.form.descricao?.trim() || undefined };
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, payload));
        this.notify.success('Processo atualizado');
      } else {
        await firstValueFrom(this.repo.criar(payload));
        this.notify.success('Processo criado');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar processo')); }
    finally { this.salvando.set(false); }
  }

  async alternarSituacao(item: Processo) {
    if (item.ativo && item.totalEtapas > item.etapasConcluidas) {
      this.notify.error('Só é possível encerrar processo com todas as etapas concluídas');
      return;
    }
    try {
      if (item.ativo) {
        await firstValueFrom(this.repo.encerrar(item.id));
        this.notify.success('Processo encerrado');
      } else {
        await firstValueFrom(this.repo.reativar(item.id));
        this.notify.success('Processo reativado');
      }
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao alterar situação do processo')); }
  }

  async excluir(item: Processo) {
    const ok = await this.confirmService.confirm('Excluir processo', `Deseja excluir o processo "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Processo excluído');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir processo')); }
  }
}
