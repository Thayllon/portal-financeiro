import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LayoutComponent } from './core/layout/layout.component';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'receitas', loadComponent: () => import('./features/receitas/receitas.component').then(m => m.ReceitasComponent) },
      { path: 'despesas', loadComponent: () => import('./features/despesas/despesas.component').then(m => m.DespesasComponent) },
      { path: 'contas', loadComponent: () => import('./features/contas/contas.component').then(m => m.ContasComponent) },
      { path: 'categorias', loadComponent: () => import('./features/categorias-receita/categorias-receita.component').then(m => m.CategoriasComponent) },
    ]
  },
  { path: '**', redirectTo: '' }
];
