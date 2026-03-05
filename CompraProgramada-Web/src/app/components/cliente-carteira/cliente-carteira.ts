import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartData, ChartType } from 'chart.js';
import { ApiService } from '../../services/api';
import { Router } from '@angular/router'; 
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-cliente-carteira',
  standalone: true,
  imports: [CommonModule, BaseChartDirective, FormsModule],
  templateUrl: './cliente-carteira.html',
  styleUrls: ['./cliente-carteira.scss']
})
export class ClienteCarteiraComponent implements OnInit {
  resumo: any = null;
  ativos: any[] = [];
  nome: string = '';
  dataConsulta: string = '';
  clienteId: number = 0;
  novoAporte: number | null = null;
  mensagemAporte: string = '';
  sucessoAporte: boolean = false;
  carregandoAporte: boolean = false;
  historicoVendas: any[] = [];
  clienteAtivo: boolean = true; 

  // Configurações do Gráfico lateral dos ativos
  public doughnutChartLabels: string[] = [];
  public doughnutChartData: ChartData<'doughnut'> = {
    labels: this.doughnutChartLabels,
    datasets: [{ data: [] }]
  };
  public doughnutChartType: ChartType = 'doughnut';

  constructor(
    private apiService: ApiService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  ngOnInit(): void {
    
    if (typeof localStorage !== 'undefined') {
      const idSalvo = localStorage.getItem('clienteId');

      if (idSalvo) {
        this.clienteId = Number(idSalvo);
        this.carregarDados();
      } else {
        
        console.warn('Sessão inválida. Redirecionando para login...');
        this.router.navigate(['/login']);
      }
    }
  }

  calcularValorTotalAplicado(ativo: any): number {
   
    const qtd = Number(ativo.quantidade || ativo.Quantidade || 0);
    const preco = Number(ativo.precoMedio || ativo.PrecoMedio || 0);
    
    return qtd * preco;
  }

  carregarHistoricoVendas() {
    this.apiService.getHistoricoVendas(this.clienteId).subscribe({
      next: (res: any) => {
        this.historicoVendas = res;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao buscar histórico de vendas:', err);
      }
    });
  }

  carregarDados() {
    this.apiService.getCarteira(this.clienteId).subscribe({
      next: (data: any) => {
        this.nome = data.nome;
        this.dataConsulta = data.dataConsulta;
        this.resumo = data.resumo;
        
        
        const ativosOriginais = this.resumo?.ativos || [];
        const ativosSeparados: any[] = [];

        ativosOriginais.forEach((ativo: any) => {
          const qtdTotal = Number(ativo.quantidade || 0);
          const tickerBase = ativo.ticker || '';
          const precoMedio = Number(ativo.precoMedio  || 0);
          const cotacaoAtual = Number(ativo.cotacaoAtual || 0);
          const plPercentual = Number(ativo.plPercentual || 0);
          const valorAtualCarteira = Number(this.resumo?.valorAtualCarteira  || 1);
          const qtdLotePadrao = Math.floor(qtdTotal / 100) * 100;
          const qtdFracionaria = qtdTotal % 100;
          const criarFatiaAtivo = (qtd: number, isFrac: boolean) => {
            const valorAtualNovo = qtd * cotacaoAtual;
            const plValorNovo = (cotacaoAtual - precoMedio) * qtd;
            const composicaoNova = valorAtualCarteira > 0 ? (valorAtualNovo / valorAtualCarteira) * 100 : 0;

            return {
              ticker: isFrac ? `${tickerBase}F` : tickerBase,
              quantidade: qtd,
              precoMedio: precoMedio,
              cotacaoAtual: cotacaoAtual,
              plValor: plValorNovo,
              plPercentual: plPercentual,
              composicaoCarteira: composicaoNova
            };
          };

          if (qtdLotePadrao > 0) {
            ativosSeparados.push(criarFatiaAtivo(qtdLotePadrao, false));
          }

          if (qtdFracionaria > 0) {
            ativosSeparados.push(criarFatiaAtivo(qtdFracionaria, true));
          }
        });

        this.ativos = ativosSeparados;

        this.doughnutChartLabels = ativosOriginais.map((a: any) => a.ticker);
        this.doughnutChartData = {
          labels: this.doughnutChartLabels,
          datasets: [{
            data: ativosOriginais.map((a: any) => Number(a.composicaoCarteira || a.ComposicaoCarteira || 0)),
            backgroundColor: [
              '#ec7000', '#0047bb', '#28a745', '#ffc107', '#17a2b8', 
              '#dc3545', '#6610f2', '#e83e8c', '#fd7e14', '#20c997'
            ],
            hoverOffset: 4
          }]
        };

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao buscar a carteira do cliente logado:', err);
      }
    });
    this.carregarHistoricoVendas();
  }

  sair() {

    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('clienteId');
    }
    this.router.navigate(['/login']);
  }
  encerrarAdesao() {
    const confirmacao = window.confirm(
      'ATENÇÃO: Tem certeza que deseja sair do produto Compra Programada?\n\n' +
      'Sua posição atual em custódia será mantida, mas as compras mensais e os rebalanceamentos automáticos serão suspensos definitivamente.'
    );

    if (confirmacao) {
      this.apiService.sairDoProduto(this.clienteId).subscribe({
        next: (res: any) => {
          alert(res.mensagem); 
          this.clienteAtivo = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          alert(err.error?.erro || 'Erro ao tentar sair do produto.');
        }
      });
    }
  }

  atualizarAporte() {
    this.mensagemAporte = '';
    this.sucessoAporte = false;

    if (!this.novoAporte || this.novoAporte < 100) {
      this.mensagemAporte = 'O valor mínimo é de R$ 100,00.';
      this.cdr.detectChanges();
      return;
    }

    

    this.carregandoAporte = true;
    this.cdr.detectChanges();

    this.apiService.atualizarAporte(this.clienteId, this.novoAporte).subscribe({
      next: (res: any) => {
        this.carregandoAporte = false;
        this.sucessoAporte = true;
        this.mensagemAporte = res.mensagem || 'Aporte atualizado com sucesso!';
        this.novoAporte = null; 
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.carregandoAporte = false;
        this.sucessoAporte = false;
        this.mensagemAporte = err.error?.erro || err.error?.Erro || 'Erro ao atualizar valor.';
        this.cdr.detectChanges();
      }
    });
  }

  limparAporteMsg() {
    if (this.mensagemAporte) {
      this.mensagemAporte = '';
      this.cdr.detectChanges();
    }
  }
}