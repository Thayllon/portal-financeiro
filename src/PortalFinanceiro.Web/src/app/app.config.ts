import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideLucideIcons } from '@lucide/angular';
import {
  LucideLayoutDashboard, LucideTrendingUp, LucideTrendingDown,
  LucideWallet, LucideCreditCard, LucideTag, LucidePlus,
  LucideChevronLeft, LucideChevronRight, LucideCheck,
  LucidePencil, LucideTrash2, LucideX, LucideSearch,
  LucideCalendar, LucideDollarSign, LucideLogOut,
  LucideChevronDown, LucideArrowLeft, LucideArrowRight,
  LucideLoader, LucideInbox, LucideAlertCircle, LucideCheckCircle,
  LucideInfo,
} from '@lucide/angular';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideLucideIcons(
      LucideLayoutDashboard, LucideTrendingUp, LucideTrendingDown,
      LucideWallet, LucideCreditCard, LucideTag, LucidePlus,
      LucideChevronLeft, LucideChevronRight, LucideCheck,
      LucidePencil, LucideTrash2, LucideX, LucideSearch,
      LucideCalendar, LucideDollarSign, LucideLogOut,
      LucideChevronDown, LucideArrowLeft, LucideArrowRight,
      LucideLoader, LucideInbox, LucideAlertCircle, LucideCheckCircle,
      LucideInfo,
    ),
  ],
};
