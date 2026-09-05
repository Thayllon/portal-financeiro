import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { UsuarioRepository } from '../../core/repositories/usuario.repository';
import { Usuario, UsuarioRequest } from '../../core/models/usuario.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { SideDrawerComponent } from '../../shared/components/side-drawer.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { SectionHeaderComponent } from '../../shared/components/section-header.component';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [FormsModule, ModalComponent, SideDrawerComponent, StatusBadgeComponent, SectionHeaderComponent, LucideDynamicIcon],
  templateUrl: './usuarios.component.html',
  styleUrl: './usuarios.component.scss'
})
export class UsuariosComponent implements OnInit {
  private repo = inject(UsuarioRepository);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private confirmService = inject(ConfirmService);

  usuarios = signal<Usuario[]>([]);
  loading = signal(true);
  modalVisible = signal(false);
  drawerVisible = signal(false);
  editando = signal<Usuario | null>(null);
  salvando = signal(false);

  form: UsuarioRequest = { nome: '', email: '', senha: '', isAdmin: false, ativo: true };

  ngOnInit() { this.carregar(); }

  async carregar() {
    this.loading.set(true);
    try {
      const data = await firstValueFrom(this.repo.listar());
      this.usuarios.set(data);
    } catch { this.notify.error('Erro ao carregar usuários'); }
    finally { this.loading.set(false); }
  }

  abrirModal(item?: Usuario) {
    if (item) {
      this.abrirDrawer(item);
      return;
    }
    this.form = { nome: '', email: '', senha: '', isAdmin: false, ativo: true };
    this.editando.set(null);
    this.modalVisible.set(true);
  }

  abrirDrawer(item: Usuario) {
    this.form = { nome: item.nome, email: item.email, senha: '', isAdmin: item.isAdmin, ativo: item.ativo };
    this.editando.set(item);
    this.modalVisible.set(false);
    this.drawerVisible.set(true);
  }

  fecharDrawer() {
    this.drawerVisible.set(false);
    this.editando.set(null);
  }

  fecharModal() {
    this.modalVisible.set(false);
    this.editando.set(null);
  }

  ehUsuarioAtual(item: Usuario): boolean {
    return item.id === this.auth.user()?.usuarioId;
  }

  async salvar() {
    if (!this.form.nome || !this.form.email) { this.notify.error('Preencha os campos obrigatórios'); return; }
    if (!this.editando() && !this.form.senha) { this.notify.error('Informe uma senha para o novo usuário'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Usuário atualizado');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Usuário criado');
      }
      this.fecharModal();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao salvar usuário')); }
    finally { this.salvando.set(false); }
  }

  async alterarAtivo(item: Usuario) {
    if (this.ehUsuarioAtual(item) && item.ativo) {
      this.notify.error('Você não pode desativar o próprio usuário');
      return;
    }
    const acao = item.ativo ? 'desativar' : 'ativar';
    const ok = await this.confirmService.confirm(`${acao[0].toUpperCase()}${acao.slice(1)} usuário`, `Deseja ${acao} "${item.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.alterarAtivo(item.id, !item.ativo));
      this.notify.success(`Usuário ${acao}do`);
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, `Erro ao ${acao} usuário`)); }
  }
}
