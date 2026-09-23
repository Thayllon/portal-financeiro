import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Dashboard, DashboardAnual } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  obter(mes: number, ano: number, idConta?: string): Observable<Dashboard> {
    const params: Record<string, string> = { mes: mes.toString(), ano: ano.toString() };
    if (idConta) params['idConta'] = idConta;
    return this.get<Dashboard>('/dashboard', params);
  }

  obterAnual(ano: number, idConta?: string): Observable<DashboardAnual> {
    const params: Record<string, string> = { ano: ano.toString() };
    if (idConta) params['idConta'] = idConta;
    return this.get<DashboardAnual>('/dashboard/anual', params);
  }
}
