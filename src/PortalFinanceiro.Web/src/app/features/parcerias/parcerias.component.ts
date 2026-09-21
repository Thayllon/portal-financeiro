import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ParceriaRepository } from '../../core/repositories/parceria.repository';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';
import { Parceria, ParceriaRequest, ResumoParceriaMensal } from '../../core/models/parceria.model';
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
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { useListPagination } from '../../shared/composables/use-list-pagination.composable';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-parcerias',
  standalone: true,
  imports: [FormsModule, ModalComponent, CustomSelectComponent, CurrencyInputDirective, ValorMascaradoPipe, PrivacidadeToggleComponent, StatusBadgeComponent, ListPaginationComponent, MonthNavComponent, LucideDynamicIcon],
  templateUrl: './parcerias.component.html',
  styleUrl: './parcerias.component.scss'
})
export class ParceriasComponent implements OnInit {
  private repo = inject(ParceriaRepository);
  private pessoaRepo = inject(PessoaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);
  private router = inject(Router);

  parcerias = signal<Parceria[]>([]);
  parceiros = signal<Pessoa[]>([]);
  clientes = signal<Pessoa[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<Parceria | null>(null);
  salvando = signal(false);
  filtroSituacao: boolean | undefined = undefined;
  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());
  resumoMensal = signal<ResumoParceriaMensal | null>(null);

  form: ParceriaRequest = { nome: '', idParceiro: '', idCliente: '', valor: 0, percentualParceiro: 50 };

  parceirosOptions = computed<SelectOption[]>(() => this.parceiros().map(p => ({ value: p.id, label: p.nome })));
  clientesOptions = computed<SelectOption[]>(() => this.clientes().map(p => ({ value: p.id, label: p.nome })));
  situacaoOptions: SelectOption[] = [
    { value: 'todas', label: 'Todas' },
    { value: 'ativas', label: 'Ativas' },
    { value: 'encerradas', label: 'Encerradas' }
  ];

  paginacao = useListPagination(this.parcerias, { initialPageSize: 10 });

  ngOnInit() { this.carregarPessoas(); this.carregar(); this.carregarResumo(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar(this.filtroSituacao));
      this.parcerias.set(data);
    } catch { this.notify.error('Erro ao carregar parcerias'); }
    finally { this.loading.set(false); }
  }

  mudarSituacao(valor: string | number | null) {
    this.filtroSituacao = valor === 'ativas' ? true : valor === 'encerradas' ? false : undefined;
    this.carregar();
  }

  navegarMes(dir: number) {
    let m = this.mes() + dir, a = this.ano();
    if (m > 12) { m = 1; a++; }
    if (m < 1) { m = 12; a--; }
    this.mes.set(m); this.ano.set(a);
    this.carregarResumo();
  }

  async carregarResumo() {
    try {
      const data = await firstValueFrom(this.repo.resumoMensal(this.ano(), this.mes()));
      this.resumoMensal.set(data);
    } catch { this.resumoMensal.set(null); }
  }

  async carregarPessoas() {
    try {
      const todas = await firstValueFrom(this.pessoaRepo.listar());
      this.parceiros.set(todas.filter(p => p.tipo === 'Parceiro'));
      this.clientes.set(todas.filter(p => p.tipo === 'Cliente'));
    } catch {}
  }

  abrirModal() {
    this.form = { nome: '', idParceiro: '', idCliente: '', valor: 0, percentualParceiro: 50 };
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  editar(item: Parceria) {
    this.form = { nome: item.nome, idParceiro: item.idParceiro, idCliente: item.idCliente, valor: item.valor, percentualParceiro: item.percentualParceiro };
    this.editando.set(item);
    this.modalVisible.set(true);
  }

  verDetalhes(item: Parceria) {
    this.router.navigate(['/parcerias', item.id]);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (!this.form.nome?.trim()) { this.notify.error('Informe o nome'); return; }
    if (!this.form.idParceiro) { this.notify.error('Selecione o parceiro'); return; }
    if (!this.form.idCliente) { this.notify.error('Selecione o cliente'); return; }
    if (!this.form.valor || this.form.valor <= 0) { this.notify.error('Informe um valor válido'); return; }
    if (this.form.percentualParceiro == null || this.form.percentualParceiro < 0 || this.form.percentualParceiro > 100) { this.notify.error('Informe um percentual entre 0 e 100'); return; }
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

  async alternarSituacao(item: Parceria) {
    if (item.ativo && (item.faltaReceber > 0 || item.faltaPagar > 0)) {
      this.notify.error('Só é possível encerrar parceria sem valores a receber e a pagar');
      return;
    }
    try {
      if (item.ativo) {
        await firstValueFrom(this.repo.encerrar(item.id));
        this.notify.success('Parceria encerrada');
      } else {
        await firstValueFrom(this.repo.reativar(item.id));
        this.notify.success('Parceria reativada');
      }
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao alterar situação da parceria')); }
  }

  async excluir(item: Parceria) {
    const ok = await this.confirmService.confirm('Excluir parceria', `Deseja excluir a parceria "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Parceria excluída');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir parceria')); }
  }
}
