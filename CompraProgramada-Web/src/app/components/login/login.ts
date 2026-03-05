import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent {
  credenciais = { email: '', senha: '' };
  mensagemErro = '';
  carregando = false;

  constructor(
    private authService: AuthService, 
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  fazerLogin() {
    this.mensagemErro = '';

    if (!this.credenciais.email || !this.credenciais.senha) {
      this.mensagemErro = 'Por favor, preencha o e-mail e a senha.';
      this.cdr.detectChanges(); 
      return;
    }

    this.carregando = true;
    this.cdr.detectChanges();

    this.authService.login(this.credenciais).subscribe({
      next: (res: any) => {
        this.carregando = false;
        
        if (res.role === 'ADMIN') {
          this.router.navigate(['/admin']);
        } else {

          if (typeof localStorage !== 'undefined') {
            localStorage.setItem('clienteId', res.clienteId); 
          }
          this.router.navigate(['/cliente']);
        }
      },
      error: (err) => {
        this.carregando = false;
        
        // TRATATIVA DO ERRO 400
        if (err.status === 400) {
          this.mensagemErro = err.error?.erro || err.error?.Erro || 'E-mail ou senha incorretos.';
        } else {
          this.mensagemErro = 'Erro de conexão com o servidor. Tente novamente.';
        }
        
        console.error('Falha no login:', err);
        this.cdr.detectChanges(); 
      }
    });
  }

  limparErro() {
    if (this.mensagemErro) {
      this.mensagemErro = '';
      this.cdr.detectChanges(); 
    }
  }
}