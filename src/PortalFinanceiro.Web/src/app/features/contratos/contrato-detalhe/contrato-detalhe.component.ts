import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ContratoRepository } from '../../../core/repositories/contrato.repository';
import { Contrato } from '../../../core/models/contrato.model';
import { Receita } from '../../../core/models/receita.model';
import { STATUS_REALIZADO } from '../../../core/models/status.model';
import { NotificationService } from '../../../core/services/notification.service';
import { CurrencyBRLPipe } from '../../../shared/pipes/currency-brl.pipe';
import { StatusBadgeComponent } from '../../../shared/components/status-badge.component';
import { ListPaginationComponent } from '../../../shared/components/list-pagination.component';
import { useListPagination } from '../../../shared/composables/use-list-pagination.composable';
import { LucideDynamicIcon } from '@lucide/angular';
import { mensagemErro } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-contrato-detalhe',
  standalone: true,
  imports: [DatePipe, CurrencyBRLPipe, StatusBadgeComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './contrato-detalhe.component.html',
  styleUrl: './contrato-detalhe.component.scss'
})
export class ContratoDetalheComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private repo = inject(ContratoRepository);
  private notify = inject(NotificationService);

  contrato = signal<Contrato | null>(null);
  receitas = signal<Receita[]>([]);
  loading = signal(true);

  receitasPaginacao = useListPagination(this.receitas, { initialPageSize: 10 });

  readonly statusRealizado = STATUS_REALIZADO;

  entradasRecebidas = computed(() => this.receitas().filter(r => r.status === STATUS_REALIZADO).reduce((s, r) => s + r.valor, 0));
  entradasPendentes = computed(() => this.receitas().filter(r => r.status !== STATUS_REALIZADO).reduce((s, r) => s + r.valor, 0));

  async ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.voltar(); return; }
    await this.carregar(id);
  }

  async carregar(id: string) {
    this.loading.set(true);
    try {
      const [contrato, receitas] = await Promise.all([
        firstValueFrom(this.repo.obter(id)),
        firstValueFrom(this.repo.listarReceitas(id))
      ]);
      this.contrato.set(contrato);
      this.receitas.set(receitas);
    } catch (e) {
      this.notify.error(mensagemErro(e, 'Erro ao carregar contrato'));
      this.voltar();
    } finally {
      this.loading.set(false);
    }
  }

  voltar() {
    this.router.navigate(['/contratos']);
  }
}
