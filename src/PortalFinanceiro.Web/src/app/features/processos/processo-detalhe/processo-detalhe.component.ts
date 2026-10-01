import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ProcessoRepository } from '../../../core/repositories/processo.repository';
import { Processo, ProcessoEtapa, ProcessoEtapaItem, ProcessoEtapaRequest, ProcessoEtapaItemRequest } from '../../../core/models/processo.model';
import { AuthService } from '../../../core/services/auth.service';
import { NivelPermissao } from '../../../core/models/permissao.model';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmService } from '../../../shared/services/confirm.service';
import { ModalComponent } from '../../../shared/components/modal.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge.component';
import { LucideDynamicIcon } from '@lucide/angular';
import { mensagemErro } from '../../../shared/utils/api-error.util';
import { diasEmAberto, duracaoDias, formatarDuracao } from '../../../shared/utils/processo.utils';

@Component({
  selector: 'app-processo-detalhe',
  standalone: true,
  imports: [DatePipe, FormsModule, ModalComponent, StatusBadgeComponent, LucideDynamicIcon],
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
  etapasAbertas = signal<Set<string>>(new Set());
  loading = signal(true);
  modalEtapaVisible = signal(false);
  modalItemVisible = signal(false);
  editandoEtapa = signal<ProcessoEtapa | null>(null);
  etapaDoItem = signal<ProcessoEtapa | null>(null);
  editandoItem = signal<ProcessoEtapaItem | null>(null);
  salvando = signal(false);

  formEtapa: ProcessoEtapaRequest = { nome: '', descricao: '', dataPrevista: undefined };
  formItem: ProcessoEtapaItemRequest = { nome: '', descricao: '', obrigatorio: true, exigeAnexo: false };

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
      const ordenadas = [...processo.etapas].sort((a, b) => a.ordem - b.ordem);
      this.etapas.set(ordenadas);
      if (this.etapasAbertas().size === 0 && ordenadas.length > 0) {
        const primeiraAberta = ordenadas.find(e => !e.concluida) ?? ordenadas[0];
        this.etapasAbertas.set(new Set([primeiraAberta.id]));
      }
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

  diasAbertos(): number {
    const p = this.processo();
    if (!p) return 0;
    return diasEmAberto(p.dataCadastro, p.dataEncerramento);
  }

  duracao(etapa: ProcessoEtapa): string {
    return formatarDuracao(duracaoDias(etapa.dataInicio, etapa.dataConclusao));
  }

  atrasada(etapa: ProcessoEtapa): boolean {
    if (etapa.concluida || !etapa.dataPrevista) return false;
    const hoje = new Date().toISOString().split('T')[0];
    return etapa.dataPrevista.split('T')[0] < hoje;
  }

  alternarEtapa(id: string) {
    const abertas = new Set(this.etapasAbertas());
    if (abertas.has(id)) abertas.delete(id);
    else abertas.add(id);
    this.etapasAbertas.set(abertas);
  }

  abrirModalEtapa() {
    this.formEtapa = { nome: '', descricao: '', dataPrevista: undefined };
    this.editandoEtapa.set(null);
    this.modalEtapaVisible.set(true);
  }

  editarEtapa(etapa: ProcessoEtapa) {
    this.formEtapa = { nome: etapa.nome, descricao: etapa.descricao ?? '', dataPrevista: etapa.dataPrevista?.split('T')[0] };
    this.editandoEtapa.set(etapa);
    this.modalEtapaVisible.set(true);
  }

  fecharModalEtapa() {
    this.modalEtapaVisible.set(false);
    this.editandoEtapa.set(null);
  }

  abrirModalItem(etapa: ProcessoEtapa, item: ProcessoEtapaItem | null) {
    this.etapaDoItem.set(etapa);
    this.editandoItem.set(item);
    this.formItem = item
      ? { nome: item.nome, descricao: item.descricao ?? '', obrigatorio: item.obrigatorio, exigeAnexo: item.exigeAnexo }
      : { nome: '', descricao: '', obrigatorio: true, exigeAnexo: false };
    this.modalItemVisible.set(true);
  }

  fecharModalItem() {
    this.modalItemVisible.set(false);
    this.etapaDoItem.set(null);
    this.editandoItem.set(null);
  }

  async salvarEtapa() {
    const id = this.processo()?.id;
    if (!id) return;
    if (!this.formEtapa.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    this.salvando.set(true);
    try {
      const payload: ProcessoEtapaRequest = { nome: this.formEtapa.nome.trim(), descricao: this.formEtapa.descricao?.trim() || undefined };
      if (this.formEtapa.dataPrevista) payload.dataPrevista = this.formEtapa.dataPrevista;
      if (this.editandoEtapa()) {
        await firstValueFrom(this.repo.atualizarEtapa(id, this.editandoEtapa()!.id, payload));
        this.notify.success('Fase atualizada');
      } else {
        await firstValueFrom(this.repo.criarEtapa(id, payload));
        this.notify.success('Fase criada');
      }
      this.fecharModalEtapa();
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar fase')); }
    finally { this.salvando.set(false); }
  }

  async salvarItem() {
    const id = this.processo()?.id;
    const etapa = this.etapaDoItem();
    if (!id || !etapa) return;
    if (!this.formItem.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    this.salvando.set(true);
    try {
      const payload: ProcessoEtapaItemRequest = {
        nome: this.formItem.nome.trim(),
        descricao: this.formItem.descricao?.trim() || undefined,
        obrigatorio: this.formItem.obrigatorio,
        exigeAnexo: this.formItem.exigeAnexo
      };
      if (this.editandoItem()) {
        await firstValueFrom(this.repo.atualizarItem(id, this.editandoItem()!.id, payload));
        this.notify.success('Item atualizado');
      } else {
        await firstValueFrom(this.repo.criarItem(id, etapa.id, payload));
        this.notify.success('Item criado');
      }
      this.fecharModalItem();
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar item')); }
    finally { this.salvando.set(false); }
  }

  async concluirEtapa(etapa: ProcessoEtapa, forcar = false): Promise<void> {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.concluirEtapa(id, etapa.id, forcar));
      this.notify.success('Fase concluída');
      await this.recarregar();
    } catch (e) {
      if (!forcar && e instanceof HttpErrorResponse && e.status === 422) {
        const ok = await this.confirmService.confirm(
          'Concluir fase mesmo assim',
          `${mensagemErro(e, 'Há itens obrigatórios pendentes')}. Deseja concluir a fase mesmo assim?`
        );
        if (ok) return this.concluirEtapa(etapa, true);
        return;
      }
      this.notify.error(mensagemErro(e, 'Erro ao concluir fase'));
    }
  }

  async estornarEtapa(etapa: ProcessoEtapa) {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.estornarEtapa(id, etapa.id));
      this.notify.success('Fase estornada');
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao estornar fase')); }
  }

  async moverEtapa(etapa: ProcessoEtapa, direcao: number) {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.moverEtapa(id, etapa.id, direcao));
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao reordenar fase')); }
  }

  async excluirEtapa(etapa: ProcessoEtapa) {
    const id = this.processo()?.id;
    if (!id) return;
    const ok = await this.confirmService.confirm('Excluir fase', `Deseja excluir a fase "${etapa.nome}" e seus itens?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluirEtapa(id, etapa.id));
      this.notify.success('Fase excluída');
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir fase')); }
  }

  async concluirItem(item: ProcessoEtapaItem) {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.concluirItem(id, item.id));
      this.notify.success('Item concluído');
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao concluir item')); }
  }

  async estornarItem(item: ProcessoEtapaItem) {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.estornarItem(id, item.id));
      this.notify.success('Item estornado');
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao estornar item')); }
  }

  async moverItem(item: ProcessoEtapaItem, direcao: number) {
    const id = this.processo()?.id;
    if (!id) return;
    try {
      await firstValueFrom(this.repo.moverItem(id, item.id, direcao));
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao reordenar item')); }
  }

  async excluirItem(etapa: ProcessoEtapa, item: ProcessoEtapaItem) {
    const id = this.processo()?.id;
    if (!id) return;
    const ok = await this.confirmService.confirm('Excluir item', `Deseja excluir o item "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluirItem(id, item.id));
      this.notify.success('Item excluído');
      await this.recarregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir item')); }
  }

  anexarEmBreve() {
    this.notify.info('Anexos chegam na próxima etapa: o botão será liberado com o armazenamento no Google Drive.');
  }
}
