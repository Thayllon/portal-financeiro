import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ContratoRepository } from '../../core/repositories/contrato.repository';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { Contrato, ContratoRequest } from '../../core/models/contrato.model';
import { Pessoa } from '../../core/models/pessoa.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { CurrencyInputDirective } from '../../shared/directives/currency-input.directive';
import { ValorMascaradoPipe } from '../../shared/pipes/valor-mascarado.pipe';
import { PrivacidadeToggleComponent } from '../../shared/components/privacidade-toggle.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ListPaginationComponent } from '../../shared/components/list-pagination.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-contratos',
  standalone: true,
  imports: [FormsModule, ModalComponent, CustomSelectComponent, CurrencyInputDirective, ValorMascaradoPipe, PrivacidadeToggleComponent, StatusBadgeComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './contratos.component.html',
  styleUrl: './contratos.component.scss'
})
export class ContratosComponent implements OnInit {
  private repo = inject(ContratoRepository);
  private pessoaRepo = inject(PessoaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private router = inject(Router);

  contratos = signal<Contrato[]>([]);
  clientes = signal<Pessoa[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Contrato | null>(null);
  salvando = signal(false);
  filtroSituacao: boolean | undefined = undefined;

  form: ContratoRequest = { nome: '', idCliente: '', valor: 0 };

  clientesOptions = computed<SelectOption[]>(() => this.clientes().map(p => ({ value: p.id, label: p.nome })));
  situacaoOptions: SelectOption[] = [
    { value: 'todas', label: 'Todas' },
    { value: 'ativas', label: 'Ativas' },
    { value: 'encerradas', label: 'Encerradas' }
  ];

  paginacao = useListPagination(this.contratos, { initialPageSize: 10 });

  ngOnInit() { this.carregarClientes(); this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar(this.filtroSituacao));
      this.contratos.set(data);
    } catch { this.notify.error('Erro ao carregar contratos'); }
    finally { this.loading.set(false); }
  }

  mudarSituacao(valor: string | number | null) {
    this.filtroSituacao = valor === 'ativas' ? true : valor === 'encerradas' ? false : undefined;
    this.carregar();
  }

  async carregarClientes() {
    try {
      const todas = await firstValueFrom(this.pessoaRepo.listar());
      this.clientes.set(todas.filter(p => p.tipo === 'Cliente'));
    } catch {}
  }

  abrirModal() {
    this.form = { nome: '', idCliente: '', valor: 0 };
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  editar(item: Contrato) {
    this.form = { nome: item.nome, idCliente: item.idCliente, valor: item.valor };
    this.editando.set(item);
    this.modalVisible.set(true);
  }

  verDetalhes(item: Contrato) {
    this.router.navigate(['/contratos', item.id]);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    if (!this.form.idCliente) { this.notify.error('Selecione o cliente'); return; }
    if (!this.form.valor || this.form.valor <= 0) { this.notify.error('Informe um valor válido'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Contrato atualizado');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Contrato criado');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar contrato')); }
    finally { this.salvando.set(false); }
  }

  async alternarSituacao(item: Contrato) {
    if (item.ativo && item.faltaReceber > 0) {
      this.notify.error('Só é possível encerrar contrato sem valores a receber');
      return;
    }
    try {
      if (item.ativo) {
        await firstValueFrom(this.repo.encerrar(item.id));
        this.notify.success('Contrato encerrado');
      } else {
        await firstValueFrom(this.repo.reativar(item.id));
        this.notify.success('Contrato reativado');
      }
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao alterar situação do contrato')); }
  }

  async excluir(item: Contrato) {
    const ok = await this.confirmService.confirm('Excluir contrato', `Deseja excluir o contrato "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Contrato excluído');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir contrato')); }
  }
}
