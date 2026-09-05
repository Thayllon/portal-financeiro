import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { LayoutComponent } from './core/layout/layout.component';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'receitas', loadComponent: () => import('./features/receitas/receitas.component').then(m => m.ReceitasComponent), canActivate: [permissionGuard('receitas')] },
      { path: 'despesas', loadComponent: () => import('./features/despesas/despesas.component').then(m => m.DespesasComponent), canActivate: [permissionGuard('despesas')] },
      { path: 'contas', loadComponent: () => import('./features/contas/contas.component').then(m => m.ContasComponent), canActivate: [permissionGuard('contas')] },
      { path: 'categorias', loadComponent: () => import('./features/categorias-receita/categorias-receita.component').then(m => m.CategoriasComponent), canActivate: [permissionGuard('categorias')] },
      { path: 'clientes', loadComponent: () => import('./features/clientes/clientes.component').then(m => m.ClientesComponent), canActivate: [permissionGuard('clientes')] },
      { path: 'parceiros', loadComponent: () => import('./features/parceiros/parceiros.component').then(m => m.ParceirosComponent), canActivate: [permissionGuard('parceiros')] },
      { path: 'usuarios', loadComponent: () => import('./features/usuarios/usuarios.component').then(m => m.UsuariosComponent), canActivate: [adminGuard] },
    ]
  },
  { path: '**', redirectTo: '' }
];
