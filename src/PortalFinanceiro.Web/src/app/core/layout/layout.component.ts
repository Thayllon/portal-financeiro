import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastComponent } from '../../shared/components/toast.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { ConfirmService } from '../../shared/services/confirm.service';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastComponent, ConfirmDialogComponent, LucideDynamicIcon],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  authService = inject(AuthService);
  private confirmService = inject(ConfirmService);
  sidebarCollapsed = signal(false);

  toggleSidebar() {
    this.sidebarCollapsed.update(v => !v);
  }

  async logout() {
    const confirmed = await this.confirmService.confirm(
      'Sair do sistema',
      'Tem certeza que deseja sair?'
    );
    if (confirmed) {
      this.authService.logout();
    }
  }

  initials(): string {
    const nome = this.authService.user()?.nome ?? '';
    return nome.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
