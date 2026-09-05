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
  loading = false;
  erro = '';
  showPassword = signal(false);
  trocandoSenha = signal(false);
  novaSenha = '';
  confirmarSenha = '';
  showNovaSenha = signal(false);
  showConfirmarSenha = signal(false);
  salvandoSenha = false;
  erroSenha = '';

  onSubmit() {
    this.loading = true;
    this.erro = '';
    this.authService.login(this.email, this.senha).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.precisaTrocarSenha) {
          this.trocandoSenha.set(true);
          return;
        }
        this.router.navigate(['/']);
      },
      error: () => {
        this.erro = 'Email ou senha inválidos.';
        this.loading = false;
      }
    });
  }

  onTrocarSenha() {
    if (this.novaSenha.length < 6) {
      this.erroSenha = 'A nova senha deve ter no mínimo 6 caracteres.';
      return;
    }
    if (this.novaSenha !== this.confirmarSenha) {
      this.erroSenha = 'As senhas não coincidem.';
      return;
    }
    this.salvandoSenha = true;
    this.erroSenha = '';
    this.authService.trocarSenha(this.email, this.senha, this.novaSenha).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => {
        this.erroSenha = 'Não foi possível alterar a senha. Tente novamente.';
        this.salvandoSenha = false;
      }
    });
  }

  voltarLogin() {
    this.trocandoSenha.set(false);
    this.novaSenha = '';
    this.confirmarSenha = '';
    this.erroSenha = '';
  }
}