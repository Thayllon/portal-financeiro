import { HttpClient } from '@angular/common/http';
import { Observable, timeout, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

export abstract class BaseHttpRepository {
  protected abstract http: HttpClient;
  protected baseUrl = environment.apiUrl;

  private sanitizeParams(params?: Record<string, any>): Record<string, string> {
    if (!params) return {};
    const sanitized: Record<string, string> = {};
    Object.keys(params).forEach(key => {
      const val = params[key];
      if (val !== undefined && val !== null && val !== '') {
        sanitized[key] = String(val);
      }
    });
    return sanitized;
  }

  protected get<T>(path: string, params?: Record<string, any>): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${path}`, { params: this.sanitizeParams(params) }).pipe(
      timeout(30000),
      catchError(err => throwError(() => err))
    );
  }

  protected post<T>(path: string, body?: any, params?: Record<string, any>): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${path}`, body, { params: this.sanitizeParams(params) }).pipe(
      timeout(30000),
      catchError(err => throwError(() => err))
    );
  }

  protected put<T>(path: string, body?: any, params?: Record<string, any>): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${path}`, body, { params: this.sanitizeParams(params) }).pipe(
      timeout(30000),
      catchError(err => throwError(() => err))
    );
  }

  protected delete<T>(path: string, params?: Record<string, any>): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${path}`, { params: this.sanitizeParams(params) }).pipe(
      timeout(30000),
      catchError(err => throwError(() => err))
    );
  }
}
