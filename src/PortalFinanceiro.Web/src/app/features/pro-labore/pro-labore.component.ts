import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ProLaboreRepository } from '../../core/repositories/pro-labore.repository';
import { ContaBancariaRepository } from '../../core/repositories/conta-bancaria.repository';
import { ProLabore, ProLaboreRequest } from '../../core/models/pro-labore.model';
import { ContaBancaria } from '../../core/models/conta-bancaria.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { CurrencyInputDirective } from '../../shared/directives/currency-input.directive';
import { CurrencyBRLPipe } from '../../shared/pipes/currency-brl.pipe';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

const MESES = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];

@Component({
  selector: 'app-pro-labore',
  standalone: true,
  imports: [FormsModule, ModalComponent, CustomSelectComponent, CurrencyInputDirective, CurrencyBRLPipe, LucideDynamicIcon],
  templateUrl: './pro-labore.component.html',
  styleUrl: './pro-labore.component.scss'
})
export class ProLaboreComponent implements OnInit {
  private repo = inject(ProLaboreRepository);
  private contaRepo = inject(ContaBancariaRepository);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  registros = signal<ProLabore[]>([]);
  contas = signal<ContaBancaria[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  editando = signal<ProLabore | null>(null);
  salvando = signal(false);

  form: ProLaboreRequest = { ano: new Date().getFullYear(), mes: new Date().getMonth() + 1, valor: 0, percentualInss: 11, idConta: '' };

  contasOptions: SelectOption[] = [];
  mesOptions: SelectOption[] = MESES.map((label, i) => ({ value: String(i + 1), label }));

  inssCalculado = computed(() => this.registros().reduce((s, r) => s + (r.valor * r.percentualInss) / 100, 0));
  totalBruto = computed(() => this.registros().reduce((s, r) => s + r.valor, 0));

  ngOnInit() { this.carregar(); }

  async carregarContas() {
    try {
      const contas = await firstValueFrom(this.contaRepo.listar());
      this.contas.set(contas);
      this.contasOptions = contas.map(c => ({ value: c.id, label: `${c.nome} (${c.banco})` }));
    } catch {}
  }

  async carregar() {
    this.loading.set(true);
    await this.carregarContas();
    try {
      this.registros.set((await firstValueFrom(this.repo.listar())).sort((a, b) => b.ano - a.ano || b.mes - a.mes));
    } catch { this.notify.error('Erro ao carregar pró-labores'); }
    finally { this.loading.set(false); }
  }

  labelMes(mes: number): string { return MESES[mes - 1] ?? ''; }
  mesString = (mes: number) => String(mes);
  setMes = (value: string) => { this.form.mes = Number(value); };

  valorInss(registro: ProLabore): number { return Math.round((registro.valor * registro.percentualInss) / 100 * 100) / 100; }

  abrirModal(item?: ProLabore) {
    if (item) {
      this.form = { ano: item.ano, mes: item.mes, valor: item.valor, percentualInss: item.percentualInss, idConta: item.idConta };
      this.editando.set(item);
    } else {
      this.form = { ano: new Date().getFullYear(), mes: new Date().getMonth() + 1, valor: 0, percentualInss: 11, idConta: '' };
      this.editando.set(null);
    }
    this.modalVisible.set(true);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  async salvar() {
    if (this.form.valor <= 0) { this.notify.error('Valor deve ser maior que zero'); return; }
    if (!this.form.idConta) { this.notify.error('Selecione a conta'); return; }
    if (this.form.percentualInss <= 0 || this.form.percentualInss >= 100) { this.notify.error('Percentual de INSS deve estar entre 0 e 100'); return; }

    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Pró-labore atualizado');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Pró-labore criado — despesa de INSS gerada');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar pró-labore')); }
    finally { this.salvando.set(false); }
  }

  async excluir(item: ProLabore) {
    const ok = await this.confirmService.confirm('Excluir pró-labore', `Deseja excluir o pró-labore de ${this.labelMes(item.mes)}/${item.ano}?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.excluir(item.id));
      this.notify.success('Pró-labore excluído — despesa de INSS removida');
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir pró-labore')); }
  }
}