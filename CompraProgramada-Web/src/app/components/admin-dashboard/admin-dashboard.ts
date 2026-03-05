import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.scss']
})
export class AdminDashboardComponent implements OnInit {
  
  // Variáveis da Cesta
  cestaAtual: any = null;
  novaCesta: any[] = [
    { ticker: '', percentual: 0 },
    { ticker: '', percentual: 0 },
    { ticker: '', percentual: 0 },
    { ticker: '', percentual: 0 },
    { ticker: '', percentual: 0 }
  ];
  historicoCestas: any[] = [];

  // Variáveis de Feedback
  mensagemErro: string = '';
  mensagemSucesso: string = '';
  
  // Variáveis do Motor de Compras
  executandoMotor: boolean = false;
  resultadoMotor: any = null;

  // Variável da Conta Master (Resíduos)
  custodiaMaster: any = null;

  constructor(
    private apiService: ApiService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

    this.carregarCestaAtual();
    this.carregarCustodiaMaster();
    this.carregarHistoricoCestas();
  }


  carregarHistoricoCestas() {
    this.apiService.getHistoricoCestas().subscribe({
      next: (res: any) => {
        this.historicoCestas = res;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Erro ao buscar histórico de cestas', err)
    });
  }
  carregarCestaAtual() {
    this.apiService.getCestaAtual().subscribe({
      next: (data: any) => {
        this.cestaAtual = data.ativos || data.Ativos || data.itens || data.Itens || data.cesta || (Array.isArray(data) ? data : []);
        
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.cestaAtual = null;
        this.cdr.detectChanges();
      }
    });
  }

  salvarNovaCesta() {
    this.mensagemErro = '';
    this.mensagemSucesso = '';

    const temTickerVazio = this.novaCesta.some(item => !item.ticker || item.ticker.trim() === '');
    if (temTickerVazio) {
      this.mensagemErro = 'Preencha o nome (Ticker) de todos os 5 ativos.';
      this.cdr.detectChanges(); 
      return;
    }

    const soma = this.novaCesta.reduce((acc, item) => acc + item.percentual, 0);
    if (soma !== 100) {
      this.mensagemErro = `A soma dos percentuais deve ser 100%. Soma atual: ${soma}%.`;
      this.cdr.detectChanges();
      return;
    }

    const payload = { ativos: this.novaCesta };

    this.apiService.atualizarCesta(payload).subscribe({
      next: (res: any) => {
        this.mensagemSucesso = res.mensagem || res.Mensagem || 'Cesta Top Five atualizada com sucesso!';
        

        this.novaCesta = [
          { ticker: '', percentual: 0 }, { ticker: '', percentual: 0 }, 
          { ticker: '', percentual: 0 }, { ticker: '', percentual: 0 }, 
          { ticker: '', percentual: 0 }
        ];

        this.carregarCestaAtual(); 
      },
      error: (err) => {
        // Tratativa Erro 400 vindo do server

        this.mensagemErro = err.error?.erro || err.error?.Erro || 'Erro inesperado ao guardar a cesta.';
        console.error('Falha ao atualizar cesta:', err);
        this.cdr.detectChanges(); 
      }
    });
  }

  dispararMotor() {
    this.executandoMotor = true;
    this.resultadoMotor = null;
    this.mensagemErro = '';
    this.mensagemSucesso = '';
    
    this.cdr.detectChanges(); 

    const dataRef = new Date().toISOString().split('T')[0];

    this.apiService.executarMotor(dataRef).subscribe({
      next: (res: any) => {
        let msgFinal = 'Motor executado com sucesso!'; 
        
        if (typeof res === 'string') {
            msgFinal = res; 
        } else if (res && res.mensagem) {
            msgFinal = res.mensagem; 
        } else if (res && res.Mensagem) {
            msgFinal = res.Mensagem; 
        }

        this.resultadoMotor = { mensagem: msgFinal };
        this.executandoMotor = false;
        
        this.carregarCustodiaMaster();
        
        this.cdr.detectChanges(); 
      },
      error: (err) => {
        this.mensagemErro = err.error?.erro || err.error?.Erro || 'Erro inesperado ao executar o motor B3.';
        this.executandoMotor = false; 
        this.cdr.detectChanges(); 
        console.error('Falha no motor:', err);
      }
    });
  }

  carregarCustodiaMaster() {
    this.apiService.getCustodiaMaster().subscribe({
      next: (res: any) => {
        this.custodiaMaster = {
          saldoFinanceiro: res.valorTotalResiduo ?? res.ValorTotalResiduo ?? 0,
          ativos: res.custodia ?? res.Custodia ?? []
        };
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao carregar a conta master:', err);
      }
    });
  }
}