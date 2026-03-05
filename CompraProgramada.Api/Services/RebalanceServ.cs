using CompraProgramada.Api.Data;
using CompraProgramada.Api.Models;
using CompraProgramada.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramada.Api.Services { 
    public class RebalanceServ {
        private readonly AppDbContext context;
        private readonly IKafkaServ kafkaServ;

        public RebalanceServ(AppDbContext contextInfo, IKafkaServ kafkaServInfo) {
            context = contextInfo;
            kafkaServ = kafkaServInfo;
        }

        public async Task ExecutarRebalanceamentoAsync(long clienteId, List<ItemCesta> novaCesta, Dictionary<string, decimal> cotacoesAtuais) {
            var cliente = await context.Clientes
            .Include(c => c.ContaGrafica)
            .ThenInclude(cg => cg!.Custodias)
            .Include(c => c.HistoricoVendas) 
                .FirstOrDefaultAsync(c => c.Id == clienteId);

            if (cliente == null) return;

            decimal patrimonioTotalEmAcoes = cliente.ContaGrafica!.Custodias
                .Sum(c => c.Quantidade * cotacoesAtuais[c.Ticker]);

            decimal volumeVendidoNesteRebalanceamento = 0;
            decimal lucroTotalNesteRebalanceamento = 0;
            
            
            var detalhesVenda = new List<object>();

            var alvosFinanceiros = novaCesta.ToDictionary(
                c => c.Ticker, 
                c => patrimonioTotalEmAcoes * (c.PercentualPeso / 100m)
            );

            // PROCESSA VENDAS E GUARDA OS DETALHES
            foreach (var custodia in cliente.ContaGrafica.Custodias.ToList()) {

                decimal precoAtual = cotacoesAtuais[custodia.Ticker];
                decimal valorAtualNaCarteira = custodia.Quantidade * precoAtual;
                decimal valorAlvo = alvosFinanceiros.ContainsKey(custodia.Ticker) ? alvosFinanceiros[custodia.Ticker] : 0;

                if (valorAtualNaCarteira > valorAlvo) {
                    decimal valorParaVender = valorAtualNaCarteira - valorAlvo;
                    int qtdParaVender = 0;

                
                    if (valorAlvo == 0) {
                        qtdParaVender = custodia.Quantidade;
                    } else {
                        
                        qtdParaVender = (int)(valorParaVender / precoAtual);
                    }

                    if (qtdParaVender <= 0) continue;

                    decimal lucroDestaOperacao = valorParaVender - (qtdParaVender * custodia.PrecoMedio);
                    
                    volumeVendidoNesteRebalanceamento += valorParaVender;
                    lucroTotalNesteRebalanceamento += lucroDestaOperacao;

                    // Anota o detalhe da venda para o Kafka
                    detalhesVenda.Add(new {
                        ticker = custodia.Ticker,
                        quantidade = Math.Round((decimal)qtdParaVender, 2),
                        precoVenda = Math.Round(precoAtual, 2),
                        precoMedio = Math.Round(custodia.PrecoMedio, 2),
                        lucro = Math.Round(lucroDestaOperacao, 2)
                    });

                    cliente.HistoricoVendas.Add(new HistoricoVenda
                    {
                        Ticker = custodia.Ticker,
                        Quantidade = qtdParaVender,
                        PrecoVenda = precoAtual,
                        PrecoMedio = custodia.PrecoMedio,
                        ValorTotal = valorParaVender,
                        Lucro = lucroDestaOperacao,
                        Data = DateTime.UtcNow
                    });

                    // Remove da custódia
                    custodia.Quantidade -= qtdParaVender;
                    if (custodia.Quantidade <= 0)
                    {
                        context.Custodias.Remove(custodia);
                    }
                }
            }

            // Motor de compra
            foreach (var alvo in alvosFinanceiros) {

                var custodia = cliente.ContaGrafica.Custodias.FirstOrDefault(c => c.Ticker == alvo.Key);
                decimal precoAtual = cotacoesAtuais[alvo.Key];
                decimal valorAtualNaCarteira = custodia != null ? (custodia.Quantidade * precoAtual) : 0;

                if (valorAtualNaCarteira < alvo.Value) {

                    decimal valorParaComprar = alvo.Value - valorAtualNaCarteira;
                    decimal qtdParaComprar = valorParaComprar / precoAtual;

                    if (custodia != null) {

                        decimal valorInvestidoAntigo = custodia.Quantidade * custodia.PrecoMedio;
                        custodia.Quantidade += (int)qtdParaComprar;
                        custodia.PrecoMedio = (valorInvestidoAntigo + valorParaComprar) / custodia.Quantidade;
                    } else {

                        cliente.ContaGrafica.Custodias.Add(new Custodia {
                            ContaGraficaId = cliente.ContaGrafica.Id,
                            Ticker = alvo.Key,
                            Quantidade = (int)qtdParaComprar, 
                            PrecoMedio = precoAtual
                        });
                    }
                }
            }

            // APURAÇÃO DE IR
            decimal vendasPassadasNesteMes = cliente.HistoricoVendas
                .Where(v => v.Data.Month == DateTime.UtcNow.Month && v.Data.Year == DateTime.UtcNow.Year)
                .Sum(v => v.ValorTotal); 

            decimal volumeTotalMes = vendasPassadasNesteMes + volumeVendidoNesteRebalanceamento;
            decimal impostoDevido = 0;
            
            lucroTotalNesteRebalanceamento = 5000m; //*************************************************************************w

            if (volumeTotalMes > 20000m && lucroTotalNesteRebalanceamento > 0)
            {
                impostoDevido = lucroTotalNesteRebalanceamento * 0.20m;

                //Payload JSON
                var payloadIr = new {
                    tipo = "IR_VENDA",
                    clienteId = cliente.Id,
                    cpf = cliente.Cpf,
                    mesReferencia = DateTime.UtcNow.ToString("yyyy-MM"),
                    totalVendasMes = Math.Round(volumeTotalMes, 2),
                    lucroLiquido = Math.Round(lucroTotalNesteRebalanceamento, 2),
                    aliquota = 0.20m,
                    valorIR = Math.Round(impostoDevido, 2),
                    detalhes = detalhesVenda,
                    dataCalculo = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") 
                };

                // Dispara o metodo do KafkaServ
                await kafkaServ.PublicarIrVendaAsync(payloadIr);
            }

            await context.SaveChangesAsync();
        }
    }
}