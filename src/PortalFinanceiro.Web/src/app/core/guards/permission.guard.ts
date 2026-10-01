import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ModuloPermissao, NivelPermissao } from '../models/permissao.model';

export const permissionGuard = (modulo: ModuloPermissao, nivelMinimo = NivelPermissao.Leitura): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      return router.parseUrl('/login');
    }

    if (authService.temPermissao(modulo, nivelMinimo)) {
      return true;
    }

    return router.parseUrl('/');
  };
};

/// Exige leitura em pelo menos um dos módulos informados.
export const permissionGuardAny = (modulos: ModuloPermissao[], nivelMinimo = NivelPermissao.Leitura): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      return router.parseUrl('/login');
    }

    if (modulos.some(m => authService.temPermissao(m, nivelMinimo))) {
      return true;
    }

    return router.parseUrl('/');
  };
};
