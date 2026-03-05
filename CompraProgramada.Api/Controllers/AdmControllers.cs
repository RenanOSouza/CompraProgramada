using CompraProgramada.Api.Data;
using CompraProgramada.Api.Models;
using CompraProgramada.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CompraProgramada.Api.Controllers {

    [ApiController]
    [Route("api/admin")]
    public class AdmController : ControllerBase {
        private readonly AppDbContext context;
        private readonly RebalanceServ rebalanceServ;
        private readonly IParserServ parser;

        public AdmController(AppDbContext contextInfo, RebalanceServ rebalanceServInfo, IParserServ parserServInfo) {
            context = contextInfo;
            rebalanceServ = rebalanceServInfo;
            parser = parserServInfo;
        }

        public class RequestItemCesta {
            required public string Ticker { get; set; }
            public decimal Percentual { get; set; }
        }

        public class AtualizarCestaRequest {
            public required List<RequestItemCesta> Ativos { get; set; }
        }

        [HttpPut("cesta-top-five")]
        
        //Realiza a verificação dos dados que entra e atualiza a cesta
        public async Task<IActionResult> AtualizarCesta([FromBody] AtualizarCestaRequest request) {
           
            if (request.Ativos == null || request.Ativos.Count != 5) {
                return BadRequest(new { erro = "A cesta não contem todos os ativos.", codigo = "QUANTIDADE_ATIVOS_INVALIDA" });
            }

           
            var somaPerc = request.Ativos.Sum(a => a.Percentual);

            if (somaPerc != 100) {
                return BadRequest(new { erro = "A soma dos percentuais deve ser exatamente 100%.", codigo = "PERCENTUAIS_INVALIDOS" });
            }

            
            if (request.Ativos.Any(a => a.Percentual <= 0)) {
                return BadRequest(new { erro = "Todos os ativos devem ter a % maior que zero.", codigo = "PERCENTUAL_ZERO_OU_NEGATIVO" });
            }

            var tickersDistintos = request.Ativos.Select(a => a.Ticker.ToUpper()).Distinct().Count();
            
            if (tickersDistintos != 5) {
                return BadRequest(new { erro = "A cesta nao pode conter ativos duplicados.", codigo = "ATIVOS_DUPLICADOS" });
            }

            var cestasAtivas = await context.TopFive.Where(c => c.Ativo).ToListAsync();
            
            foreach (var cesta in cestasAtivas) {
                cesta.Ativo = false;
            }

            // Cria a nova cesta
            var novaCesta = new TopFive{
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                Itens = request.Ativos.Select(a => new ItemCesta {
                    Ticker = a.Ticker.ToUpper(),
                    PercentualPeso = a.Percentual
                }).ToList()
            };

             
            context.TopFive.Add(novaCesta);
            await context.SaveChangesAsync();

            
            var clientesAtivos = await context.Clientes.Where(c => c.Ativo).ToListAsync();

            if (clientesAtivos.Any())
            {
                var tickersNaNovaCesta = novaCesta.Itens.Select(i => i.Ticker).ToList();
                var tickersNaCustodia = await context.Custodias.Select(c => c.Ticker).Distinct().ToListAsync();
                
                var todosTickers = tickersNaNovaCesta.Union(tickersNaCustodia).Distinct().ToList();

                var caminhoArquivo = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "cotacoes", "COTAHIST_D27022026.TXT"));
                var cotacoesExtraidas = parser.ExtrairCotacoes(caminhoArquivo, todosTickers);

                var cotacoesAtuais = new Dictionary<string, decimal>();
                foreach (var ticker in todosTickers)
                {
                    var infoB3 = cotacoesExtraidas.FirstOrDefault(c => c.Ticker == ticker);
                    cotacoesAtuais[ticker] = infoB3?.PrecoFechamento ?? 1m; 
                }

                foreach (var cliente in clientesAtivos) {
                    try {

                        await rebalanceServ.ExecutarRebalanceamentoAsync(cliente.Id, novaCesta.Itens.ToList(), cotacoesAtuais);
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"[ERRO REBALANCEAMENTO] Falha no cliente {cliente.Id}: {ex.Message}");
                    }
                }
            }

            return Ok(new { mensagem = "Cesta Top Five atualizada com sucesso.", cestaId = novaCesta.Id });
        }

        [HttpGet("cesta/atual")]
        public async Task<IActionResult> ConsultarCestaAtual() {

            var cesta = await context.TopFive
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Ativo);

            if (cesta == null) {
                return NotFound(new { erro = "Nenhuma cesta ativa encontrada.", codigo = "CESTA_NAO_ENCONTRADA" });
            }

            return Ok(new {
                cestaId = cesta.Id,
                dataCriacao = cesta.DataCriacao,
                
                itens = cesta.Itens.Select(i => new { 
                    ticker = i.Ticker, 
                    percentual = i.PercentualPeso 
                })
            });
        }

        [HttpGet("historico")]
        public async Task<IActionResult> ObterHistoricoCestas() {

            var historico = await context.TopFive
                .Include(c => c.Itens)
                .OrderByDescending(c => c.Id) 
                .ToListAsync();

            var response = historico.Select(c => new {
                id = c.Id,
                ativo = c.Ativo,
                itens = c.Itens.Select(i => new {
                    ticker = i.Ticker,
                    percentualPeso = i.PercentualPeso
                }).ToList()
            });

            return Ok(response);
        }

        [HttpGet("conta-master/custodia")]
        public async Task<IActionResult> ConsultarCustodiaMaster() {
            
            var contaMaster = await context.ContasGraficas
                .Include(c => c.Custodias)
                .FirstOrDefaultAsync(c => c.Tipo == "MASTER");

            if (contaMaster == null) {
                return NotFound(new { erro = "Conta Master nao encontrada.", codigo = "MASTER_NAO_ENCONTRADA" });
            }

            var custodias = contaMaster.Custodias ?? new List<Custodia>();
            decimal valorTotalResiduo = 0;
            var listaCustodiaResponse = new List<object>();

            foreach (var item in custodias) {

                decimal cotacaoAtual = item.PrecoMedio;
                decimal valorAtual = item.Quantidade * cotacaoAtual;
                
                valorTotalResiduo += valorAtual;

                listaCustodiaResponse.Add(new {
                    ticker = item.Ticker,
                    quantidade = item.Quantidade,
                    precoMedio = Math.Round(item.PrecoMedio, 2),
                    valorAtual = Math.Round(valorAtual, 2),
                    origem = $"Residuo de distribuicoes anteriores"
                });
            }

            return Ok(new {
                contaMaster = new {
                    id = contaMaster.Id,
                    numeroConta = contaMaster.NumeroConta,
                    tipo = contaMaster.Tipo
                },
                custodia = listaCustodiaResponse,
                valorTotalResiduo = Math.Round(contaMaster.SaldoFinanceiro, 2)
            });
        }
    }
    
}


