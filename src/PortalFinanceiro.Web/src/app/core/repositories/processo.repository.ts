import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpRepository } from './base-http.repository';
import { Processo, ProcessoRequest, ProcessoEtapa, ProcessoEtapaRequest, ProcessoEtapaItem, ProcessoEtapaItemRequest } from '../models/processo.model';

@Injectable({ providedIn: 'root' })
export class ProcessoRepository extends BaseHttpRepository {
  protected http = inject(HttpClient);

  listar(ativo?: boolean): Observable<Processo[]> {
    return this.get<Processo[]>('/processos', { ...(ativo !== undefined ? { ativo } : {}) });
  }

  obter(id: string): Observable<Processo> {
    return this.get<Processo>(`/processos/${id}`);
  }

  criar(data: ProcessoRequest): Observable<Processo> {
    return this.post<Processo>('/processos', data);
  }

  atualizar(id: string, data: ProcessoRequest): Observable<Processo> {
    return this.put<Processo>(`/processos/${id}`, data);
  }

  encerrar(id: string): Observable<any> {
    return this.put<any>(`/processos/${id}/encerrar`);
  }

  reativar(id: string): Observable<any> {
    return this.put<any>(`/processos/${id}/reativar`);
  }

  excluir(id: string): Observable<any> {
    return this.delete<any>(`/processos/${id}`);
  }

  criarEtapa(id: string, data: ProcessoEtapaRequest): Observable<ProcessoEtapa> {
    return this.post<ProcessoEtapa>(`/processos/${id}/etapas`, data);
  }

  atualizarEtapa(id: string, etapaId: string, data: ProcessoEtapaRequest): Observable<ProcessoEtapa> {
    return this.put<ProcessoEtapa>(`/processos/${id}/etapas/${etapaId}`, data);
  }

  concluirEtapa(id: string, etapaId: string, forcar = false): Observable<ProcessoEtapa> {
    return this.put<ProcessoEtapa>(`/processos/${id}/etapas/${etapaId}/concluir`, {}, { ...(forcar ? { forcar: true } : {}) });
  }

  estornarEtapa(id: string, etapaId: string): Observable<ProcessoEtapa> {
    return this.put<ProcessoEtapa>(`/processos/${id}/etapas/${etapaId}/estornar`);
  }

  moverEtapa(id: string, etapaId: string, direcao: number): Observable<any> {
    return this.put<any>(`/processos/${id}/etapas/${etapaId}/mover`, {}, { direcao });
  }

  excluirEtapa(id: string, etapaId: string): Observable<any> {
    return this.delete<any>(`/processos/${id}/etapas/${etapaId}`);
  }

  criarItem(id: string, etapaId: string, data: ProcessoEtapaItemRequest): Observable<ProcessoEtapaItem> {
    return this.post<ProcessoEtapaItem>(`/processos/${id}/etapas/${etapaId}/itens`, data);
  }

  atualizarItem(id: string, itemId: string, data: ProcessoEtapaItemRequest): Observable<ProcessoEtapaItem> {
    return this.put<ProcessoEtapaItem>(`/processos/${id}/itens/${itemId}`, data);
  }

  concluirItem(id: string, itemId: string): Observable<ProcessoEtapaItem> {
    return this.put<ProcessoEtapaItem>(`/processos/${id}/itens/${itemId}/concluir`);
  }

  estornarItem(id: string, itemId: string): Observable<ProcessoEtapaItem> {
    return this.put<ProcessoEtapaItem>(`/processos/${id}/itens/${itemId}/estornar`);
  }

  moverItem(id: string, itemId: string, direcao: number): Observable<any> {
    return this.put<any>(`/processos/${id}/itens/${itemId}/mover`, {}, { direcao });
  }

  excluirItem(id: string, itemId: string): Observable<any> {
    return this.delete<any>(`/processos/${id}/itens/${itemId}`);
  }
}
