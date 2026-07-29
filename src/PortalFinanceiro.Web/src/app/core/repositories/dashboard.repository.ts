import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Dashboard } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  obter(idUsuario: string, mes: number, ano: number): Observable<Dashboard> {
    return this.get<Dashboard>('/dashboard', { idUsuario, mes, ano });
  }
}
