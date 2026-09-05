import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { UsuarioRepository } from '../../core/repositories/usuario.repository';
import { Usuario, UsuarioRequest } from '../../core/models/usuario.model';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ModalComponent } from '../../shared/components/modal.component';
import { SideDrawerComponent } from '../../shared/components/side-drawer.component';
import { CustomSelectComponent, SelectOption } from '../../shared/components/custom-select.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { mensagemErro } from '../../shared/utils/api-error.util';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [FormsModule, ModalComponent, SideDrawerComponent, CustomSelectComponent, StatusBadgeComponent, LucideDynamicIcon],
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
  fluxoAdicional = signal(false);
  buscaPermissao = signal('');
  dadosAberto = signal(false);
  permissoesAberto = signal(false);
  especiaisAberto = signal(false);

  permLevels: Record<string, 'none' | 'read' | 'write'> = {};

  modulosPermissao = [
    { id: 'dashboard', nome: 'Dashboard', descricao: 'Acesso aos painéis e indicadores do sistema.', icone: 'chart-line' },
    { id: 'receitas', nome: 'Receitas', descricao: 'Gestão de receitas e lançamentos financeiros.', icone: 'trending-up' },
    { id: 'despesas', nome: 'Despesas', descricao: 'Gestão de despesas e pagamentos.', icone: 'trending-down' },
    { id: 'contas', nome: 'Contas bancárias', descricao: 'Cadastro e gerenciamento de contas.', icone: 'wallet' },
    { id: 'categorias', nome: 'Categorias', descricao: 'Cadastro e organização de categorias.', icone: 'tag' },
    { id: 'clientes', nome: 'Clientes', descricao: 'Cadastro e gerenciamento de clientes.', icone: 'users' },
    { id: 'parceiros', nome: 'Parceiros', descricao: 'Cadastro e gerenciamento de parceiros.', icone: 'handshake' },
    { id: 'usuarios', nome: 'Usuários', descricao: 'Gerenciamento de usuários e permissões.', icone: 'users' },
  ];

  modulosFiltrados = computed(() => {
    const busca = this.buscaPermissao().toLowerCase();
    return this.modulosPermissao.filter(m =>
      m.nome.toLowerCase().includes(busca) || m.descricao.toLowerCase().includes(busca)
    );
  });

  form: UsuarioRequest = { nome: '', email: '', senha: '', isAdmin: false, ativo: true };

  perfilOptions: SelectOption[] = [
    { value: 'false', label: 'Usuário' },
    { value: 'true', label: 'Admin' },
  ];

  statusOptions: SelectOption[] = [
    { value: 'true', label: 'Ativo' },
    { value: 'false', label: 'Inativo' },
  ];

  perfilBloqueado = computed(() => {
    const u = this.editando();
    return !!u && (u.isAdmin || this.ehUsuarioAtual(u));
  });

  statusBloqueado = computed(() => {
    const u = this.editando();
    return !!u && this.ehUsuarioAtual(u);
  });

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
    this.buscaPermissao.set('');
    this.permLevels = {};
    this.modulosPermissao.forEach(m => {
      this.permLevels[m.id] = item.isAdmin ? 'write' : (m.id === 'dashboard' ? 'read' : 'none');
    });
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

  alternarFluxoAdicional(event: Event) {
    const ligado = (event.target as HTMLInputElement).checked;
    this.fluxoAdicional.set(ligado);
  }

  alternarPermissao(moduloId: string, nivel: 'none' | 'read' | 'write') {
    const u = this.editando();
    if (u?.isAdmin) return;
    this.permLevels[moduloId] = nivel;
  }

  async excluirAtual() {
    const u = this.editando();
    if (!u) return;
    if (this.ehUsuarioAtual(u)) {
      this.notify.error('Você não pode excluir o próprio usuário');
      return;
    }
    const ok = await this.confirmService.confirm('Excluir usuário', `Deseja excluir "${u.nome}"?`);
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.alterarAtivo(u.id, false));
      this.notify.success('Usuário excluído');
      this.fecharDrawer();
      await this.carregar();
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao excluir usuário')); }
  }

  async resetarSenhaAtual() {
    const u = this.editando();
    if (!u) return;
    const ok = await this.confirmService.confirm(
      'Reset de senha',
      `A senha de "${u.nome}" será resetada para a senha padrão (123456). Deseja continuar?`
    );
    if (!ok) return;
    try {
      await firstValueFrom(this.repo.resetarSenha(u.id));
      this.notify.success(`Senha de "${u.nome}" resetada para 123456`);
    } catch (e) { this.notify.error(mensagemErro(e, 'Erro ao resetar senha')); }
  }

  async salvar() {
    if (!this.form.nome || !this.form.email) { this.notify.error('Preencha os campos obrigatórios'); return; }
    this.salvando.set(true);
    try {
      if (this.editando()) {
        await firstValueFrom(this.repo.atualizar(this.editando()!.id, this.form));
        this.notify.success('Usuário atualizado');
      } else {
        await firstValueFrom(this.repo.criar(this.form));
        this.notify.success('Usuário criado');
      }
      this.fecharDrawer();
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
