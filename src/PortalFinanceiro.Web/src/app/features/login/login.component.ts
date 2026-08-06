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
  senha = '123456';
  loading = false;
  erro = '';
  showPassword = signal(false);

  onSubmit() {
    this.loading = true;
    this.erro = '';
    this.authService.login(this.email, this.senha).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => {
        this.erro = 'Email ou senha inválidos.';
        this.loading = false;
      }
    });
  }
}
