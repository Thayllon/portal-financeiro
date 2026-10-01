import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ProcessoRepository } from '../../../core/repositories/processo.repository';
import { Processo, ProcessoEtapa, ProcessoEtapaRequest } from '../../../core/models/processo.model';
import { AuthService } from '../../../core/services/auth.service';
import { NivelPermissao } from '../../../core/models/permissao.model';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmService } from '../../../shared/services/confirm.service';
import { ModalComponent } from '../../../shared/components/modal.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge.component';
import { ListPaginationComponent } from '../../../shared/components/list-pagination.component';
import { useListPagination } from '../../../shared/composables/use-list-pagination.composable';
import { LucideDynamicIcon } from '@lucide/angular';
import { mensagemErro } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-processo-detalhe',
  standalone: true,
  imports: [DatePipe, FormsModule, ModalComponent, StatusBadgeComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './processo-detalhe.component.html',
  styleUrl: './processo-detalhe.component.scss'
})
export class ProcessoDetalheComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private repo = inject(ProcessoRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private auth = inject(AuthService);

  podeEscrever = computed(() => this.auth.temPermissao('processos', NivelPermissao.Escrita));

  processo = signal<Processo | null>(null);
  etapas = signal<ProcessoEtapa[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<ProcessoEtapa | null>(null);
  salvando = signal(false);

  form: ProcessoEtapaRequest = { nome: '', descricao: '', dataPrevista: undefined };

  etapasPaginacao = useListPagination(this.etapas, { initialPageSize: 10 });

  async ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.voltar(); return; }
    await this.carregar(id);
  }

  async carregar(id: string) {
    this.loading.set(true);
    try {
      const processo = await firstValueFrom(this.repo.obter(id));
      this.processo.set(processo);
      this.etapas.set([...processo.etapas].sort((a, b) => a.ordem - b.ordem));
    } catch (e) {
      this.notify.error(mensagemErro(e, 'Erro ao carregar processo'));
      this.voltar();
    } finally {
      this.loading.set(false);
    }
  }

  recarregar() {
    const id = this.processo()?.id;
    if (id) return this.carregar(id);
    return Promise.resolve();
  }

  voltar() {
    this.router.navigate(['/processos']);
  }

  atrasada(etapa: ProcessoEtapa): boolean {
    if (etapa.concluida || !etapa.dataPrevista) return false;
    const hoje = new Date().toISOString().split('T')[0];
    return etapa.dataPrevista.split('T')[0] < hoje;
  }

  abrirModal() {
    this.form = { nome: '', descricao: '', dataPrevista: undefined };
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  editar(etapa: ProcessoEtapa) {
    this.form = { nome: etapa.nome, descricao: etapa.descricao ?? '', dataPrevista: etapa.dataPrevista?.split('T')[0] };
    this.editando.set(etapa);
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    const id = this.processo()?.id;
    if (!id) return;
    if (!this.form.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    this.salvando.set(true);
    try {
      const payload: ProcessoEtapaRequest = { nome: this.form.nome.trim(), descricao: this.form.descricao?.trim() || undefined };
      if (this.form.dataPrevista) payload.dataPrevista = this.form.dataPrevista;
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizarEtapa(id, this.editando()!.id, payload));
        this.notify.success('Etapa atualizada');
      } else {
        await firstValueFrom(this.repo.criarEtapa(id, payload));
        this.notify.success('Etapa criada');
      }
      this.fecharModal();
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar etapa')); }
    finally { this.salvando.set(false); }
  }

  async concluir(etapa: ProcessoEtapa) {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.concluirEtapa(id, etapa.id));
      this.notify.success('Etapa concluída');
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao concluir etapa')); }
  }

  async estornar(etapa: ProcessoEtapa) {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.estornarEtapa(id, etapa.id));
      this.notify.success('Etapa estornada');
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao estornar etapa')); }
  }

  async mover(etapa: ProcessoEtapa, direcao: number) {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.moverEtapa(id, etapa.id, direcao));
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao reordenar etapa')); }
  }

  async excluir(etapa: ProcessoEtapa) {
    const id = this.processo()?.id;
    if (!id) return;
    const ok = await this.confirmService.confirm('Excluir etapa', `Deseja excluir a etapa "${etapa.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluirEtapa(id, etapa.id));
      this.notify.success('Etapa excluída');
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir etapa')); }
  }
}
