import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, LucideDynamicIcon],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = 'admin@portal.com';
  senha = '';
  loading = signal(false);
  erro = signal('');
  showPassword = signal(false);
  trocandoSenha = signal(false);
  novaSenha = '';
  confirmarSenha = '';
  showNovaSenha = signal(false);
  showConfirmarSenha = signal(false);
  salvandoSenha = signal(false);
  erroSenha = signal('');

  onSubmit() {
    this.loading.set(true);
    this.erro.set('');
    this.authService.login(this.email, this.senha).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.precisaTrocarSenha) {
          this.trocandoSenha.set(true);
          return;
        }
        this.router.navigate(['/']);
      },
      error: () => {
        this.erro.set('Dados inválidos.');
        this.loading.set(false);
      }
    });
  }

  onTrocarSenha() {
    if (this.novaSenha.length < 6) {
      this.erroSenha.set('A nova senha deve ter no mínimo 6 caracteres.');
      return;
    }
    if (this.novaSenha !== this.confirmarSenha) {
      this.erroSenha.set('As senhas não coincidem.');
      return;
    }
    this.salvandoSenha.set(true);
    this.erroSenha.set('');
    this.authService.trocarSenha(this.email, this.senha, this.novaSenha).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => {
        this.erroSenha.set('Não foi possível alterar a senha. Tente novamente.');
        this.salvandoSenha.set(false);
      }
    });
  }

  voltarLogin() {
    this.trocandoSenha.set(false);
    this.novaSenha = '';
    this.confirmarSenha = '';
    this.erroSenha.set('');
  }
}