import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ModeloProcessoRepository } from '../../../core/repositories/modelo-processo.repository';
import { ModeloProcesso, ModeloProcessoRequest, ModeloEtapa, ModeloEtapaRequest, ModeloItem, ModeloItemRequest } from '../../../core/models/modelo-processo.model';
import { AuthService } from '../../../core/services/auth.service';
import { NivelPermissao } from '../../../core/models/permissao.model';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmService } from '../../../shared/services/confirm.service';
import { ModalComponent } from '../../../shared/components/modal.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge.component';
import { mensagemErro } from '../../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

type ModalAlvo = 'modelo' | 'etapa' | 'item' | null;

@Component({
  selector: 'app-modelos',
  standalone: true,
  imports: [FormsModule, ModalComponent, StatusBadgeComponent, LucideDynamicIcon],
  templateUrl: './modelos.component.html',
  styleUrl: './modelos.component.scss'
})
export class ModelosComponent implements OnInit {
  private repo = inject(ModeloProcessoRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private router = inject(Router);
  private auth = inject(AuthService);

  podeEscrever = computed(() => this.auth.temPermissao('processos', NivelPermissao.Escrita));

  modelos = signal<ModeloProcesso[]>([]);
  loading = signal(true);
  selecionado = signal<ModeloProcesso | null>(null);

  modalAlvo = signal<ModalAlvo>(null);
  editandoEtapa = signal<ModeloEtapa | null>(null);
  editandoItem = signal<{ etapa: ModeloEtapa; item: ModeloItem } | null>(null);
  salvando = signal(false);

  formModelo: ModeloProcessoRequest = { nome: '', descricao: '' };
  formEtapa: ModeloEtapaRequest = { nome: '', descricao: '' };
  formItem: ModeloItemRequest = { nome: '', descricao: '', obrigatorio: true, exigeAnexo: false };

  etapasAbertas = signal<Set<string>>(new Set());

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar());
      this.modelos.set(data);
      const atual = this.selecionado();
      if (atual) {
        const recarregado = data.find(m => m.id === atual.id) ?? await firstValueFrom(this.repo.obter(atual.id));
        this.selecionado.set(recarregado);
      }
    } catch { this.notify.error('Erro ao carregar modelos'); }
    finally { this.loading.set(false); }
  }

  voltar() {
    this.router.navigate(['/processos']);
  }

  async selecionar(modelo: ModeloProcesso) {
    try {
      this.selecionado.set(await firstValueFrom(this.repo.obter(modelo.id)));
      const etapas = this.selecionado()!.etapas;
      this.etapasAbertas.set(new Set(etapas.length > 0 ? [etapas[0].id] : []));
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao abrir modelo')); }
  }

  fecharEditor() {
    this.selecionado.set(null);
  }

  alternarEtapa(id: string) {
    const abertas = new Set(this.etapasAbertas());
    if (abertas.has(id)) abertas.delete(id);
    else abertas.add(id);
    this.etapasAbertas.set(abertas);
  }

  abrirModalModelo(editar: ModeloProcesso | null) {
    this.formModelo = editar ? { nome: editar.nome, descricao: editar.descricao ?? '' } : { nome: '', descricao: '' };
    this.modalAlvo.set('modelo');
  }

  abrirModalEtapa(etapa: ModeloEtapa | null) {
    this.editandoEtapa.set(etapa);
    this.formEtapa = etapa ? { nome: etapa.nome, descricao: etapa.descricao ?? '' } : { nome: '', descricao: '' };
    this.modalAlvo.set('etapa');
  }

  abrirModalItem(etapa: ModeloEtapa, item: ModeloItem | null) {
    this.editandoEtapa.set(etapa);
    this.editandoItem.set(item ? { etapa, item } : null);
    this.formItem = item
      ? { nome: item.nome, descricao: item.descricao ?? '', obrigatorio: item.obrigatorio, exigeAnexo: item.exigeAnexo }
      : { nome: '', descricao: '', obrigatorio: true, exigeAnexo: false };
    this.modalAlvo.set('item');
  }

  fecharModal() {
    this.modalAlvo.set(null);
    this.editandoEtapa.set(null);
    this.editandoItem.set(null);
  }

  async salvar() {
    const alvo = this.modalAlvo();
    if (alvo === 'modelo') return this.salvarModelo();
    if (alvo === 'etapa') return this.salvarEtapa();
    if (alvo === 'item') return this.salvarItem();
  }

  private async salvarModelo() {
    if (!this.formModelo.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    this.salvando.set(true);
    try {
      const payload: ModeloProcessoRequest = { nome: this.formModelo.nome.trim(), descricao: this.formModelo.descricao?.trim() || undefined };
      const atual = this.selecionado();
      if (atual) {
        this.selecionado.set(await firstValueFrom(this.repo.atualizar(atual.id, payload)));
        this.notify.success('Modelo atualizado');
      } else {
        const criado = await firstValueFrom(this.repo.criar(payload));
        this.notify.success('Modelo criado');
        await this.selecionar(criado);
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar modelo')); }
    finally { this.salvando.set(false); }
  }

  private async salvarEtapa() {
    const modelo = this.selecionado();
    if (!modelo) return;
    if (!this.formEtapa.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    this.salvando.set(true);
    try {
      const payload: ModeloEtapaRequest = { nome: this.formEtapa.nome.trim(), descricao: this.formEtapa.descricao?.trim() || undefined };
      if (this.editandoEtapa()) {
        await firstValueFrom(this.repo.atualizarEtapa(modelo.id, this.editandoEtapa()!.id, payload));
        this.notify.success('Fase atualizada');
      } else {
        await firstValueFrom(this.repo.criarEtapa(modelo.id, payload));
        this.notify.success('Fase criada');
      }
      this.fecharModal();
      await this.selecionar(modelo);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar fase')); }
    finally { this.salvando.set(false); }
  }

  private async salvarItem() {
    const modelo = this.selecionado();
    const etapa = this.editandoEtapa();
    if (!modelo || !etapa) return;
    if (!this.formItem.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    this.salvando.set(true);
    try {
      const payload: ModeloItemRequest = {
        nome: this.formItem.nome.trim(),
        descricao: this.formItem.descricao?.trim() || undefined,
        obrigatorio: this.formItem.obrigatorio,
        exigeAnexo: this.formItem.exigeAnexo
      };
      const edit = this.editandoItem();
      if (edit) {
        await firstValueFrom(this.repo.atualizarItem(modelo.id, edit.item.id, payload));
        this.notify.success('Item atualizado');
      } else {
        await firstValueFrom(this.repo.criarItem(modelo.id, etapa.id, payload));
        this.notify.success('Item criado');
      }
      this.fecharModal();
      await this.selecionar(modelo);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar item')); }
    finally { this.salvando.set(false); }
  }

  async excluirModelo(modelo: ModeloProcesso) {
    const ok = await this.confirmService.confirm('Excluir modelo', `Deseja excluir o modelo "${modelo.nome}"? Processos já criados a partir dele não são afetados.`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(modelo.id));
      this.notify.success('Modelo excluído');
      if (this.selecionado()?.id === modelo.id) this.fecharEditor();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir modelo')); }
  }

  async duplicar(modelo: ModeloProcesso) {
    try {
      const copia = await firstValueFrom(this.repo.duplicar(modelo.id));
      this.notify.success('Modelo duplicado');
      await this.carregar();
      await this.selecionar(copia);
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao duplicar modelo')); }
  }

  async excluirEtapa(etapa: ModeloEtapa) {
    const modelo = this.selecionado();
    if (!modelo) return;
    const ok = await this.confirmService.confirm('Excluir fase', `Deseja excluir a fase "${etapa.nome}" e seus itens?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluirEtapa(modelo.id, etapa.id));
      this.notify.success('Fase excluída');
      await this.selecionar(modelo);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir fase')); }
  }

  async moverEtapa(etapa: ModeloEtapa, direcao: number) {
    const modelo = this.selecionado();
    if (!modelo) return;
    try {
      await firstValueFrom(this.repo.moverEtapa(modelo.id, etapa.id, direcao));
      await this.selecionar(modelo);
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao reordenar fase')); }
  }

  async excluirItem(etapa: ModeloEtapa, item: ModeloItem) {
    const modelo = this.selecionado();
    if (!modelo) return;
    const ok = await this.confirmService.confirm('Excluir item', `Deseja excluir o item "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluirItem(modelo.id, item.id));
      this.notify.success('Item excluído');
      await this.selecionar(modelo);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir item')); }
  }

  async moverItem(item: ModeloItem, direcao: number) {
    const modelo = this.selecionado();
    if (!modelo) return;
    try {
      await firstValueFrom(this.repo.moverItem(modelo.id, item.id, direcao));
      await this.selecionar(modelo);
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao reordenar item')); }
  }
}
