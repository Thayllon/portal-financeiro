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
      subtitulo: 'Gestão principal das movimentações financeiras',
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
          icon: 'banknote-arrow-up',
          route: '/receitas',
          modulo: 'receitas'
        },
        {
          title: 'Despesas',
          description: 'Controle de despesas, vencimentos e pagamentos.',
          icon: 'banknote-arrow-down',
          route: '/despesas',
          modulo: 'despesas'
        }
      ]
    },
    {
      titulo: 'CONTAS E ORGANIZAÇÃO',
      subtitulo: 'Gerencie suas contas bancárias e organize suas categorias',
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
          icon: 'tag',
          route: '/categorias',
          modulo: 'categorias'
        }
      ]
    },
    {
      titulo: 'CADASTROS',
      subtitulo: 'Mantenha suas informações sempre atualizadas',
      cards: [
        {
          title: 'Clientes',
          description: 'Cadastro de clientes vinculados às receitas.',
          icon: 'building',
          route: '/clientes',
          modulo: 'clientes'
        },
        {
          title: 'Parceiros',
          description: 'Gestão de parceiros e suas informações.',
          icon: 'handshake',
          route: '/parceiros',
          modulo: 'parceiros'
        },
        {
          title: 'Parcerias',
          description: 'Percentuais e vínculos de parcerias com receitas.',
          icon: 'hand-coins',
          route: '/parcerias',
          modulo: 'parcerias'
        }
      ]
    },
    {
      titulo: 'ADMINISTRAÇÃO',
      subtitulo: 'Controle de acessos e permissões',
      cards: [
        {
          title: 'Usuários',
          description: 'Controle de usuários, permissões e acessos ao sistema.',
          icon: 'shield-check',
          route: '/usuarios',
          modulo: 'usuarios'
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
          if (card.modulo === 'dashboard') return true;
          if (!card.modulo) return true;
          return this.auth.temPermissao(card.modulo);
        })
      }))
      .filter(secao => secao.cards.length > 0);
  });
}
