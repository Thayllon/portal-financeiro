import { HttpErrorResponse } from '@angular/common/http';

export function mensagemErro(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as { message?: string; code?: string } | null;
    if (body && typeof body.message === 'string' && body.message.trim()) {
      return body.message;
    }
  }
  return fallback;
}