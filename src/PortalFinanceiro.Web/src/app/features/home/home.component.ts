import { Component, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ModuleCardComponent, ModuleCardItem } from '../../shared/components/module-card/module-card.component';

interface Secao {
  titulo: string;
  subtitulo?: string;
  cards: ModuleCardItem[];
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ModuleCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  private auth = inject(AuthService);

  private sections = signal<Secao[]>([
    {
      titulo: 'FINANCEIRO',
      subtitulo: 'Receitas, despesas, contratos e parcerias',
      cards: [
        {
          title: 'Dashboard',
          description: 'Visão consolidada de receitas, despesas e saldo por mês, conta e categoria.',
          icon: 'chart-line',
          route: '/dashboard',
          modulo: 'dashboard'
        },
        {
          title: 'Receitas',
          description: 'Cadastro e acompanhamento de receitas, parcelas e recorrências.',
          icon: 'trending-up',
          route: '/receitas',
          modulo: 'receitas'
        },
        {
          title: 'Despesas',
          description: 'Controle de despesas, vencimentos e pagamentos.',
          icon: 'trending-down',
          route: '/despesas',
          modulo: 'despesas'
        },
        {
          title: 'Parcerias',
          description: 'Percentuais e vínculos de parcerias com receitas.',
          icon: 'user-round-group',
          route: '/parcerias',
          modulo: 'parcerias'
        },
        {
          title: 'Contratos',
          description: 'Contratos com clientes e receitas vinculadas.',
          icon: 'briefcase-business',
          route: '/contratos',
          modulo: 'contratos'
        }
      ]
    },
    {
      titulo: 'CADASTROS',
      subtitulo: 'Contas, categorias e pessoas que estruturam a operação',
      cards: [
        {
          title: 'Contas Bancárias',
          description: 'Gerencie suas contas PF e PJ vinculadas aos lançamentos.',
          icon: 'wallet',
          route: '/contas',
          modulo: 'contas'
        },
        {
          title: 'Categorias',
          description: 'Organize receitas e despesas por categorias e subcategorias.',
          icon: 'tags',
          route: '/categorias',
          modulo: 'categorias'
        },
        {
          title: 'Clientes',
          description: 'Cadastro de clientes vinculados às receitas.',
          icon: 'hand-helping',
          route: '/clientes',
          modulo: 'clientes'
        },
        {
          title: 'Parceiros',
          description: 'Gestão de parceiros e suas informações.',
          icon: 'handshake',
          route: '/parceiros',
          modulo: 'parceiros'
        }
      ]
    },
    {
      titulo: 'ACESSO',
      subtitulo: 'Controle de acessos e permissões',
      cards: [
        {
          title: 'Usuários',
          description: 'Controle de usuários, permissões e acessos ao sistema.',
          icon: 'users',
          route: '/usuarios',
          modulo: 'usuarios'
        },
        {
          title: 'Testes e QA',
          description: 'Diagnóstico técnico das regras de negócio e débitos.',
          icon: 'flask-conical',
          route: '/testes',
          modulo: 'qa'
        }
      ]
    }
  ]);

  secoesVisiveis = computed(() => {
    const isAdmin = this.auth.isAdmin();
    return this.sections()
      .map(secao => ({
        ...secao,
        cards: secao.cards.filter(card => {
          if (card.modulo === 'usuarios') return isAdmin;
          if (card.modulo === 'qa') return isAdmin && this.auth.temQA();
          if (card.modulo === 'dashboard') return true;
          if (!card.modulo) return true;
          return this.auth.temPermissao(card.modulo);
        })
      }))
      .filter(secao => secao.cards.length > 0);
  });
}
