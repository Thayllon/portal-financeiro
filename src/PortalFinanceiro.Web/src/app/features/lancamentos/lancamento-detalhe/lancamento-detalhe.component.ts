import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ReceitaRepository, DespesaRepository } from '../../../core/repositories/lancamento.repository';
import { Receita } from '../../../core/models/receita.model';
import { Despesa } from '../../../core/models/despesa.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { NivelPermissao } from '../../../core/models/permissao.model';
import { ValorMascaradoPipe } from '../../../shared/pipes/valor-mascarado.pipe';
import { PrivacidadeToggleComponent } from '../../../shared/components/privacidade-toggle.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge.component';
import { LucideDynamicIcon } from '@lucide/angular';
import { mensagemErro } from '../../../shared/utils/api-error.util';
import { LancamentoTipo } from '../lancamento-listagem.component';

type ItemDetalhe = Receita | Despesa;

@Component({
  selector: 'app-lancamento-detalhe',
  standalone: true,
  imports: [DatePipe, ValorMascaradoPipe, PrivacidadeToggleComponent, StatusBadgeComponent, LucideDynamicIcon],
  templateUrl: './lancamento-detalhe.component.html',
  styleUrl: './lancamento-detalhe.component.scss'
})
export class LancamentoDetalheComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private receitaRepo = inject(ReceitaRepository);
  private despesaRepo = inject(DespesaRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);

  tipo = signal<LancamentoTipo>('receita');
  item = signal<ItemDetalhe | null>(null);
  loading = signal(true);

  ehReceita = computed(() => this.tipo() === 'receita');

  config = computed(() => {
    const receita = this.ehReceita();
    return {
      titulo: receita ? 'Receita' : 'Despesa',
      rotaLista: receita ? '/receitas' : '/despesas',
      rotuloRealizado: receita ? 'Recebido' : 'Pago',
      statusRealizadoLabel: receita ? 'Recebida' : 'Paga',
      icone: receita ? 'trending-up' : 'trending-down'
    };
  });

  podeEscrever = computed(() => this.auth.temPermissao(this.ehReceita() ? 'receitas' : 'despesas', NivelPermissao.Escrita));

  receita = computed(() => this.ehReceita() ? this.item() as Receita | null : null);
  idContrato = computed(() => this.receita()?.idContrato);
  contrato = computed(() => this.receita()?.contrato ?? '');
  parceiro = computed(() => this.receita()?.parceiro ?? '');

  async ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    const tipo = this.route.snapshot.data['tipo'] as LancamentoTipo | undefined;
    if (tipo) this.tipo.set(tipo);
    if (!id) { this.voltar(); return; }
    await this.carregar(id);
  }

  async carregar(id: string) {
    this.loading.set(true);
    try {
      const item = this.ehReceita()
        ? await firstValueFrom(this.receitaRepo.obter(id))
        : await firstValueFrom(this.despesaRepo.obter(id));
      this.item.set(item);
    } catch (e) {
      this.notify.error(mensagemErro(e, `Erro ao carregar ${this.config().titulo.toLowerCase()}`));
      this.voltar();
    } finally {
      this.loading.set(false);
    }
  }

  editar() {
    this.router.navigate([this.config().rotaLista], { queryParams: { editar: this.item()?.id } });
  }

  voltar() {
    this.router.navigate([this.config().rotaLista]);
  }
}