import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cadastro.html',
  styleUrls: ['./cadastro.scss']
})
export class CadastroComponent {
  novoUsuario = {
    nome: '',
    cpf: '',
    email: '',
    valorMensal: null
  };

  mensagemErro = '';
  carregando = false;

  constructor(
    private authService: AuthService, 
    private router: Router,
    private cdr: ChangeDetectorRef 
  ) { }

  fazerCadastro() {
    this.mensagemErro = '';
    
    
    if (!this.novoUsuario.nome || !this.novoUsuario.cpf || !this.novoUsuario.email || !this.novoUsuario.valorMensal) {
      this.mensagemErro = 'Por favor, preencha todos os campos.';
      this.cdr.detectChanges(); // Atualiza a tela
      return;
    }

    this.carregando = true;
    this.cdr.detectChanges();

    this.authService.cadastrar(this.novoUsuario).subscribe({
      next: (res: any) => {
        this.carregando = false;
        alert('Conta criada com sucesso! Use seu E-mail e CPF para entrar.');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.carregando = false;
        
        // TRATATIVA EXATA DO ERRO 400 DO C#
        if (err.status === 400) {

          this.mensagemErro = err.error?.erro || err.error?.Erro || 'Dados inválidos. Verifique as informações.';
        } else {
          this.mensagemErro = 'Erro de conexão com o servidor. Tente novamente.';
        }
        
        console.error('Falha no cadastro:', err);
        
        this.cdr.detectChanges(); 
      }
    });
  }
}