import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NivelPermissao } from '../models/permissao.model';

export const permissionGuard = (modulo: string, nivelMinimo = NivelPermissao.Leitura): CanActivateFn => {
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
