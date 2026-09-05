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
  LucideLoader, LucideInbox,
  LucideInfo, LucideChevronsLeft, LucideChevronsRight, LucideDoorOpen,
  LucideHandCoins, LucideCalendar1, LucideCalendarDays,
  LucideFolder, LucideFile, LucideReceipt, LucideRepeat,
    LucideCalendarClock, LucideEye, LucideEyeOff, LucideCopy,
  LucideBarChart3, LucideCalendarRange, LucideUsers, LucideUserX, LucideUserCheck, LucideCoins,
  LucideHandshake, LucideUserKey, LucideMonitorCog, LucideChartLine, LucideCircleAlert, LucideCircleCheck, LucideUser, LucideLock, LucideShieldCheck, LucideUserCog, LucideRotateCcwKey, LucideUserStar, LucideSparkles,
  } from '@lucide/angular';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    provideLucideIcons(
      LucideLayoutDashboard, LucideTrendingUp, LucideTrendingDown,
      LucideWallet, LucideCreditCard, LucideTag, LucidePlus,
      LucideChevronLeft, LucideChevronRight, LucideCheck,
      LucidePencil, LucideTrash2, LucideX, LucideSearch,
      LucideCalendar, LucideDollarSign, LucideLogOut,
      LucideChevronDown, LucideArrowLeft, LucideArrowRight,
  LucideLoader, LucideInbox,
      LucideInfo, LucideChevronsLeft, LucideChevronsRight, LucideDoorOpen,
      LucideHandCoins, LucideCalendar1, LucideCalendarDays,
      LucideFolder, LucideFile, LucideReceipt, LucideRepeat,
      LucideCalendarClock, LucideEye, LucideEyeOff, LucideCopy,
      LucideBarChart3, LucideCalendarRange, LucideUsers, LucideUserX, LucideUserCheck, LucideCoins,
LucideHandshake, LucideUserKey, LucideMonitorCog, LucideChartLine, LucideCircleAlert, LucideCircleCheck, LucideUser, LucideLock, LucideShieldCheck, LucideUserCog, LucideRotateCcwKey, LucideUserStar, LucideSparkles,
    ),
  ],
};
