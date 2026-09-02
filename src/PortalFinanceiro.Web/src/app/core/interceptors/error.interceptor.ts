import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isLoggingOut = false;

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError(err => {
      if (err.status === 401 && !isLoggingOut) {
        isLoggingOut = true;
        authService.logout();
      }
      return throwError(() => err);
    })
  );
};
