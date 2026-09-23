import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Diagnostico } from '../models/diagnostico.model';

@Injectable({ providedIn: 'root' })
export class DiagnosticoRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  gerar(): Observable<Diagnostico> {
    return this.get<Diagnostico>('/diagnostico');
  }
}
