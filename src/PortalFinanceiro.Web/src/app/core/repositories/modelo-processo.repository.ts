import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { ModeloProcesso, ModeloProcessoRequest, ModeloEtapa, ModeloEtapaRequest, ModeloItem, ModeloItemRequest } from '../models/modelo-processo.model';

@Injectable({ providedIn: 'root' })
export class ModeloProcessoRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(ativo?: boolean): Observable<ModeloProcesso[]> {
    return this.get<ModeloProcesso[]>('/modelos-processos', { ...(ativo !== undefined ? { ativo } : {}) });
  }

  obter(id: string): Observable<ModeloProcesso> {
    return this.get<ModeloProcesso>(`/modelos-processos/${id}`);
  }

  criar(data: ModeloProcessoRequest): Observable<ModeloProcesso> {
    return this.post<ModeloProcesso>('/modelos-processos', data);
  }

  atualizar(id: string, data: ModeloProcessoRequest): Observable<ModeloProcesso> {
    return this.put<ModeloProcesso>(`/modelos-processos/${id}`, data);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/modelos-processos/${id}`);
  }

  duplicar(id: string): Observable<ModeloProcesso> {
    return this.post<ModeloProcesso>(`/modelos-processos/${id}/duplicar`, {});
  }

  criarEtapa(id: string, data: ModeloEtapaRequest): Observable<ModeloEtapa> {
    return this.post<ModeloEtapa>(`/modelos-processos/${id}/etapas`, data);
  }

  atualizarEtapa(id: string, etapaId: string, data: ModeloEtapaRequest): Observable<ModeloEtapa> {
    return this.put<ModeloEtapa>(`/modelos-processos/${id}/etapas/${etapaId}`, data);
  }

  moverEtapa(id: string, etapaId: string, direcao: number): Observable<any> {
    return this.put<any>(`/modelos-processos/${id}/etapas/${etapaId}/mover`, {}, { direcao });
  }

  excluirEtapa(id: string, etapaId: string): Observable<any> {
    return this.delete<any>(`/modelos-processos/${id}/etapas/${etapaId}`);
  }

  criarItem(id: string, etapaId: string, data: ModeloItemRequest): Observable<ModeloItem> {
    return this.post<ModeloItem>(`/modelos-processos/${id}/etapas/${etapaId}/itens`, data);
  }

  atualizarItem(id: string, itemId: string, data: ModeloItemRequest): Observable<ModeloItem> {
    return this.put<ModeloItem>(`/modelos-processos/${id}/itens/${itemId}`, data);
  }

  moverItem(id: string, itemId: string, direcao: number): Observable<any> {
    return this.put<any>(`/modelos-processos/${id}/itens/${itemId}/mover`, {}, { direcao });
  }

  excluirItem(id: string, itemId: string): Observable<any> {
    return this.delete<any>(`/modelos-processos/${id}/itens/${itemId}`);
  }
}
