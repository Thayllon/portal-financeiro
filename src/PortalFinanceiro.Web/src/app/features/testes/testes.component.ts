import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { DiagnosticoRepository } from '../../core/repositories/diagnostico.repository';
import { Diagnostico } from '../../core/models/diagnostico.model';
import { NotificationService } from '../../core/services/notification.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-testes',
  standalone: true,
  imports: [DatePipe, StatusBadgeComponent, LucideDynamicIcon],
  templateUrl: './testes.component.html',
  styleUrl: './testes.component.scss'
})
export class TestesComponent implements OnInit {
  private repo = inject(DiagnosticoRepository);
  private notify = inject(NotificationService);

  dados = signal<Diagnostico | null>(null);
  loading = signal(true);
  atualizando = signal(false);

  totalCenarios = computed(() => (this.dados()?.regras ?? []).reduce((s, r) => s + r.cenarios.length, 0));
  cenariosPassando = computed(() => (this.dados()?.regras ?? []).reduce((s, r) => s + r.cenarios.filter(c => c.passou).length, 0));
  regrasPassando = computed(() => (this.dados()?.regras ?? []).filter(r => r.passou).length);

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      this.dados.set(await firstValueFrom(this.repo.gerar()));
    } catch (e) {
      this.notify.error(mensagemErro(e, 'Erro ao gerar diagnóstico'));
    } finally {
      this.loading.set(false);
    }
  }

  async atualizar() {
    this.atualizando.set(true);
    try {
      this.dados.set(await firstValueFrom(this.repo.gerar()));
      this.notify.success('Diagnóstico atualizado');
    } catch (e) {
      this.notify.error(mensagemErro(e, 'Erro ao atualizar diagnóstico'));
    } finally {
      this.atualizando.set(false);
    }
  }
}
