import { HttpErrorResponse } from '@angular/common/http';

interface ApiError {
  codigo?: string;
  mensagem?: string;
  tipo?: string;
  code?: string;
  message?: string;
  title?: string;
  errors?: Record<string, string[]>;
}

export function mensagemErro(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const body = (error.error ?? null) as ApiError | null;
  if (body && typeof body === 'object') {
    const mensagem = body.mensagem ?? body.message ?? body.title;
    if (typeof mensagem === 'string' && mensagem.trim()) {
      return mensagem;
    }

    if (body.errors) {
      const primeira = Object.values(body.errors)
        .flat()
        .find((m) => typeof m === 'string' && m.trim());
      if (primeira) {
        return primeira;
      }
    }
  }

  return fallback;
}
