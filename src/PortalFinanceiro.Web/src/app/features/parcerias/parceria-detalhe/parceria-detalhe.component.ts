import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ParceriaRepository } from '../../../core/repositories/parceria.repository';
import { Parceria } from '../../../core/models/parceria.model';
import { Receita } from '../../../core/models/receita.model';
import { Despesa } from '../../../core/models/despesa.model';
import { STATUS_REALIZADO } from '../../../core/models/status.model';
import { NotificationService } from '../../../core/services/notification.service';
import { ValorMascaradoPipe } from '../../../shared/pipes/valor-mascarado.pipe';
import { PrivacidadeToggleComponent } from '../../../shared/components/privacidade-toggle.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge.component';
import { CollapsibleSectionComponent } from '../../../shared/components/collapsible-section.component';
import { ListPaginationComponent } from '../../../shared/components/list-pagination.component';
import { useListPagination } from '../../../shared/composables/use-list-pagination.composable';
import { LucideDynamicIcon } from '@lucide/angular';
import { mensagemErro } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-parceria-detalhe',
  standalone: true,
  imports: [DatePipe, ValorMascaradoPipe, PrivacidadeToggleComponent, StatusBadgeComponent, CollapsibleSectionComponent, ListPaginationComponent, LucideDynamicIcon],
  templateUrl: './parceria-detalhe.component.html',
  styleUrl: './parceria-detalhe.component.scss'
})
export class ParceriaDetalheComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private repo = inject(ParceriaRepository);
  private notify = inject(NotificationService);

  parceria = signal<Parceria | null>(null);
  receitas = signal<Receita[]>([]);
  despesas = signal<Despesa[]>([]);
  loading = signal(true);

  receitasPaginacao = useListPagination(this.receitas, { initialPageSize: 10 });
  despesasPaginacao = useListPagination(this.despesas, { initialPageSize: 10 });

  readonly statusRealizado = STATUS_REALIZADO;

  entradasRecebidas = computed(() => this.parceria()?.totalRecebido ?? 0);
  entradasPendentes = computed(() => this.parceria()?.faltaReceber ?? 0);
  saidasPagas = computed(() => this.parceria()?.totalPago ?? 0);
  saidasPendentes = computed(() => this.parceria()?.faltaPagar ?? 0);

  pctRecebido = computed(() => {
    const p = this.parceria();
    if (!p || !p.valor) return 0;
    return Math.min(100, Math.max(0, (p.totalRecebido / p.valor) * 100));
  });

  pctPago = computed(() => {
    const p = this.parceria();
    if (!p || !p.valorParceiro) return 0;
    return Math.min(100, Math.max(0, (p.totalPago / p.valorParceiro) * 100));
  });

  async ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.voltar(); return; }
    await this.carregar(id);
  }

  async carregar(id: string) {
    this.loading.set(true);
    try {
      const [parceria, receitas, despesas] = await Promise.all([
        firstValueFrom(this.repo.obter(id)),
        firstValueFrom(this.repo.listarReceitas(id)),
        firstValueFrom(this.repo.listarDespesas(id))
      ]);
      this.parceria.set(parceria);
      this.receitas.set(receitas);
      this.despesas.set(despesas);
    } catch (e) {
      this.notify.error(mensagemErro(e, 'Erro ao carregar parceria'));
      this.voltar();
    } finally {
      this.loading.set(false);
    }
  }

  voltar() {
    this.router.navigate(['/parcerias']);
  }
}
