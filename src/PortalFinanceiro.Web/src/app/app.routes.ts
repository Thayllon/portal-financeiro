import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { permissionGuard, permissionGuardAny } from './core/guards/permission.guard';
import { qaGuard } from './core/guards/qa.guard';
import { LayoutComponent } from './core/layout/layout.component';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [permissionGuard('dashboard')] },
      { path: 'receitas', loadComponent: () => import('./features/receitas/receitas.component').then(m => m.ReceitasComponent), canActivate: [permissionGuard('receitas')] },
      { path: 'receitas/:id', loadComponent: () => import('./features/lancamentos/lancamento-detalhe/lancamento-detalhe.component').then(m => m.LancamentoDetalheComponent), data: { tipo: 'receita' }, canActivate: [permissionGuard('receitas')] },
      { path: 'despesas', loadComponent: () => import('./features/despesas/despesas.component').then(m => m.DespesasComponent), canActivate: [permissionGuard('despesas')] },
      { path: 'despesas/:id', loadComponent: () => import('./features/lancamentos/lancamento-detalhe/lancamento-detalhe.component').then(m => m.LancamentoDetalheComponent), data: { tipo: 'despesa' }, canActivate: [permissionGuard('despesas')] },
      { path: 'parcerias', loadComponent: () => import('./features/parcerias/parcerias.component').then(m => m.ParceriasComponent), canActivate: [permissionGuard('parcerias')] },
      { path: 'parcerias/:id', loadComponent: () => import('./features/parcerias/parceria-detalhe/parceria-detalhe.component').then(m => m.ParceriaDetalheComponent), canActivate: [permissionGuard('parcerias')] },
      { path: 'contratos', loadComponent: () => import('./features/contratos/contratos.component').then(m => m.ContratosComponent), canActivate: [permissionGuard('contratos')] },
      { path: 'contratos/:id', loadComponent: () => import('./features/contratos/contrato-detalhe/contrato-detalhe.component').then(m => m.ContratoDetalheComponent), canActivate: [permissionGuard('contratos')] },
      { path: 'processos', loadComponent: () => import('./features/processos/processos.component').then(m => m.ProcessosComponent), canActivate: [permissionGuard('processos')] },
      { path: 'processos/modelos', loadComponent: () => import('./features/processos/modelos/modelos.component').then(m => m.ModelosComponent), canActivate: [permissionGuard('processos')] },
      { path: 'processos/:id', loadComponent: () => import('./features/processos/processo-detalhe/processo-detalhe.component').then(m => m.ProcessoDetalheComponent), canActivate: [permissionGuard('processos')] },
      { path: 'contas', loadComponent: () => import('./features/contas/contas.component').then(m => m.ContasComponent), canActivate: [permissionGuard('contas')] },
      { path: 'categorias', loadComponent: () => import('./features/categorias-receita/categorias-receita.component').then(m => m.CategoriasComponent), canActivate: [permissionGuardAny(['categorias-receita', 'categorias-despesa', 'categorias-servico'])] },
      { path: 'clientes', loadComponent: () => import('./features/clientes/clientes.component').then(m => m.ClientesComponent), canActivate: [permissionGuard('clientes')] },
      { path: 'parceiros', loadComponent: () => import('./features/parceiros/parceiros.component').then(m => m.ParceirosComponent), canActivate: [permissionGuard('parceiros')] },
      { path: 'usuarios', loadComponent: () => import('./features/usuarios/usuarios.component').then(m => m.UsuariosComponent), canActivate: [adminGuard] },
      { path: 'testes', loadComponent: () => import('./features/testes/testes.component').then(m => m.TestesComponent), canActivate: [qaGuard] },
    ]
  },
  { path: '**', redirectTo: '' }
];
