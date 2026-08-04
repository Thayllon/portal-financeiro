import { Component, inject, signal, OnInit } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DashboardRepository } from '../../core/repositories/dashboard.repository';
import { Dashboard, PrevisaoMensal } from '../../core/models/dashboard.model';
import { NotificationService } from '../../core/services/notification.service';
import { SkeletonComponent } from '../../shared/components/skeleton.component';
import { MonthNavComponent } from '../../shared/components/month-nav.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { CurrencyBRLPipe } from '../../shared/pipes/currency-brl.pipe';
import { LucideDynamicIcon } from '@lucide/angular';

const MESES = ['Janeiro','Fevereiro','Março','Abril','Maio','Junho','Julho','Agosto','Setembro','Outubro','Novembro','Dezembro'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [SkeletonComponent, MonthNavComponent, StatusBadgeComponent, CurrencyBRLPipe, LucideDynamicIcon],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private repo = inject(DashboardRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);

  data = signal<Dashboard | null>(null);
  loading = signal(true);
  mes = signal(new Date().getMonth() + 1);
  ano = signal(new Date().getFullYear());

  readonly MESES = MESES;

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      this.data.set(await firstValueFrom(this.repo.obter(this.mes(), this.ano())));
    } catch { this.notify.error('Erro ao carregar dashboard'); }
    finally { this.loading.set(false); }
  }

  navegarMes(dir: number) {
    let m = this.mes() + dir, a = this.ano();
    if (m > 12) { m = 1; a++; }
    if (m < 1) { m = 12; a--; }
    this.mes.set(m); this.ano.set(a);
    this.carregar();
  }
}
