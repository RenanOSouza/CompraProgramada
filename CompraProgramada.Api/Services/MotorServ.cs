using CompraProgramada.Api.Data;
using CompraProgramada.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramada.Api.Services {
    public class MotorServ {
        private readonly AppDbContext context;
        private readonly IParserServ parser;
        private readonly IKafkaServ kafka;

        public MotorServ(AppDbContext contextInfo, IParserServ parserInfo, IKafkaServ kafkaInfo) {
            context = contextInfo;
            parser = parserInfo;
            kafka = kafkaInfo;
        }

        public async Task<string> ExecutarMotorAsync(string caminhoFicheiro) {
            var cesta = await context.TopFive
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Ativo);
            
            if (cesta == null) throw new Exception("Nenhuma Cesta Top Five ativa encontrada.");

            var tickersCesta = cesta.Itens.Select(i => i.Ticker).ToList();
            
            var contaMaster = await context.ContasGraficas.FirstOrDefaultAsync(c => c.Tipo == "MASTER");
            if (contaMaster == null) throw new Exception("Conta Master não encontrada!");

            var cotacoes = parser.ExtrairCotacoes(caminhoFicheiro, tickersCesta);
            if (cotacoes.Count != 5) throw new Exception("Não foi possível encontrar a cotação para todos os 5 ativos.");

            var clientes = await context.Clientes
                .Include(c => c.ContaGrafica)
                .Where(c => c.Ativo)
                .ToListAsync();

            if (!clientes.Any()) return "Nenhum cliente ativo para processar.";

            var ordens = new List<OrdemCompra>();
            var distribuicoes = new List<Distribuicao>();
            
            decimal totalTrocoRetidoMaster = 0; 

            //Realiza o calculo dos valores por itens na cesta
            foreach (var item in cesta.Itens) {
                var cotacao = cotacoes.First(c => c.Ticker == item.Ticker);
                int quantidadeTotalParaComprar = 0;
                decimal valorTotalOperacao = 0;

                var ordem = new OrdemCompra {
                    DataOperacao = DateTime.UtcNow,
                    Ticker = item.Ticker,
                    PrecoUnitario = cotacao.PrecoFechamento,
                    QuantidadeTotal = 0,
                    ValorTotalOperacao = 0
                };

                foreach (var cliente in clientes) {

                    decimal valorDisponivelNestaExecucao = cliente.ValorMensal / 3m;
                    decimal valorParaEstaAcao = valorDisponivelNestaExecucao * (item.PercentualPeso / 100m);
                    
                    int quantidadeParaCliente = (int)Math.Floor(valorParaEstaAcao / cotacao.PrecoFechamento);
                    decimal valorRateado = quantidadeParaCliente * cotacao.PrecoFechamento;

                    decimal trocoCliente = valorParaEstaAcao - valorRateado;
                    totalTrocoRetidoMaster += trocoCliente;

                    if (quantidadeParaCliente > 0) {

                        quantidadeTotalParaComprar += quantidadeParaCliente;
                        valorTotalOperacao += valorRateado;

                        distribuicoes.Add(new Distribuicao {

                            ContaGraficaId = cliente.ContaGrafica!.Id,
                            Ticker = item.Ticker,
                            Quantidade = quantidadeParaCliente,
                            ValorRateado = valorRateado,
                            OrdemCompra = ordem
                        });

                        var custodia = await context.Custodias
                            .FirstOrDefaultAsync(c => c.ContaGraficaId == cliente.ContaGrafica.Id && c.Ticker == item.Ticker);

                        if (custodia == null) {

                            context.Custodias.Add(new Custodia {

                                ContaGraficaId = cliente.ContaGrafica.Id,
                                Ticker = item.Ticker,
                                Quantidade = quantidadeParaCliente,
                                PrecoMedio = cotacao.PrecoFechamento,
                                DataUltimaAtualizacao = DateTime.UtcNow
                            });
                        }
                        else {
                            var valorTotalAnterior = custodia.Quantidade * custodia.PrecoMedio;
                            var valorTotalNovo = quantidadeParaCliente * cotacao.PrecoFechamento;
                            
                            custodia.Quantidade += quantidadeParaCliente;
                            custodia.PrecoMedio = (valorTotalAnterior + valorTotalNovo) / custodia.Quantidade;
                            custodia.DataUltimaAtualizacao = DateTime.UtcNow;
                        }
                    }
                }
                
                if (quantidadeTotalParaComprar > 0) {
                    var custodiaMaster = await context.Custodias
                        .FirstOrDefaultAsync(c => c.ContaGraficaId == contaMaster.Id && c.Ticker == cotacao.Ticker);

                    int estoqueMaster = custodiaMaster?.Quantidade ?? 0;
                    int necessidadeReal = quantidadeTotalParaComprar - estoqueMaster;

                    if (necessidadeReal > 0) {
                        int quantidadeLotePadrao = (necessidadeReal / 100) * 100; 
                        int quantidadeFracionaria = necessidadeReal % 100; 

                        if (quantidadeLotePadrao > 0)
                        {
                            ordens.Add(new OrdemCompra
                            {
                                Ticker = cotacao.Ticker, 
                                DataOperacao = DateTime.UtcNow,
                                QuantidadeTotal = quantidadeLotePadrao,
                                ValorTotalOperacao = quantidadeLotePadrao * cotacao.PrecoFechamento,
                                PrecoUnitario = cotacao.PrecoFechamento
                            });
                        }

                        if (quantidadeFracionaria > 0) {
                            ordens.Add(new OrdemCompra
                            {
                                Ticker = $"{cotacao.Ticker}F", 
                                DataOperacao = DateTime.UtcNow,
                                QuantidadeTotal = quantidadeFracionaria,
                                ValorTotalOperacao = quantidadeFracionaria * cotacao.PrecoFechamento,
                                PrecoUnitario = cotacao.PrecoFechamento
                            });
                        }
                    }

                    int novoEstoqueMaster = estoqueMaster + necessidadeReal - quantidadeTotalParaComprar;

                    if (custodiaMaster == null && novoEstoqueMaster > 0) {
                        context.Custodias.Add(new Custodia {
                            ContaGraficaId = contaMaster.Id,
                            Ticker = cotacao.Ticker,
                            Quantidade = novoEstoqueMaster,
                            PrecoMedio = cotacao.PrecoFechamento
                        });
                    } else if (custodiaMaster != null) {
                        custodiaMaster.Quantidade = novoEstoqueMaster;
                        context.Custodias.Update(custodiaMaster);
                    }
                }
            }
            
            contaMaster.SaldoFinanceiro += totalTrocoRetidoMaster;
            context.ContasGraficas.Update(contaMaster);
            context.OrdensCompra.AddRange(ordens);
            context.Distribuicoes.AddRange(distribuicoes);
            await context.SaveChangesAsync();

            foreach (var dist in distribuicoes) {
                await kafka.PublicarIrDedoDuroAsync(dist.ContaGraficaId, dist.Ticker, dist.ValorRateado);
            }

            return $"Motor executado com sucesso! Foram geradas {ordens.Count} ordens de compra.";
        }
    }
}