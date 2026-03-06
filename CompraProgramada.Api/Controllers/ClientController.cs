using CompraProgramada.Api.Data;
using CompraProgramada.Api.Models;
using CompraProgramada.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramada.Api.Controllers {
    
    [ApiController]
    [Route("api/[controller]")]

    public class ClientesController : ControllerBase {
        private readonly AppDbContext context;
        private readonly IParserServ parser; 

        // DI do DB
        public ClientesController(AppDbContext contextInfo, IParserServ parserInfo) {
            context = contextInfo;
            parser = parserInfo;
        }

        

        [HttpPost("{clienteId}/saida")]
        public async Task<IActionResult> SairDoProduto(long clienteId) {
            var cliente = await context.Clientes.FindAsync(clienteId);

            if (cliente == null) {
                return NotFound(new { erro = "Cliente não encontrado.", codigo = "CLIENTE_NAO_ENCONTRADO" });
            }

            if (!cliente.Ativo) {

                return BadRequest(new { erro = "Cliente saiu do produto.", codigo = "CLIENTE_JA_INATIVO" });
            }

            cliente.Ativo = false;
            await context.SaveChangesAsync();

            return Ok(new
            {
                clienteId = cliente.Id,
                nome = cliente.Nome,
                ativo = cliente.Ativo,
                dataSaida = DateTime.UtcNow,
                mensagem = "Adesão encerrada. Sua posição em custódia foi mantida."
            });
        }

        public class RequestMudarValor {
            public decimal NovoValorMensal { get; set; }
        }

        [HttpPut("{id}/valor-mensal")]
        public async Task<IActionResult> AlterarValor(long id, [FromBody] RequestMudarValor request) {
            
            

            if (request.NovoValorMensal < 100) {
                return BadRequest(new { erro = "O valor mensal mínimo é de R$ 100,00.", codigo = "VALOR_MENSAL_INVALIDO" });
            }

            var cliente = await context.Clientes.FindAsync(id);
            
            if (cliente == null) {
                return NotFound(new { erro = "Cliente não encontrado.", codigo = "CLIENTE_NAO_ENCONTRADO" });
            }

            if (!cliente.Ativo) {
                return BadRequest(new { erro = "Cliente desativado.", codigo = "CLIENTE_DESATIVADO" });
            }

            var valorAntigo = cliente.ValorMensal;

            var historico = new HistoricoValorMensal
            {
                ClienteId = cliente.Id,
                ValorAntigo = valorAntigo,
                ValorNovo = request.NovoValorMensal,
                DataAlteracao = DateTime.UtcNow
            };

            context.HistoricoValoresMensais.Add(historico);
            cliente.ValorMensal = request.NovoValorMensal;
            await context.SaveChangesAsync();

            return Ok(new {
                clienteId = cliente.Id,
                valorMensalAnterior = valorAntigo,
                valorMensalNovo = cliente.ValorMensal,
                dataAlteracao = historico.DataAlteracao,
                mensagem = "Valor mensal atualizado. O novo valor sera considerado a partir da proxima data de compra."
            });
        }

        [HttpGet("{id}/historico-vendas")]
        public async Task<IActionResult> ObterHistoricoVendas(long id)
        {
            // Verifica se o cliente existe
            var clienteExiste = await context.Clientes.AnyAsync(c => c.Id == id);
            if (!clienteExiste)
                return NotFound(new { erro = "Cliente não encontrado." });

            var historico = await context.HistoricoVendas
                .Where(h => h.ClienteId == id)
                .OrderByDescending(h => h.Data)
                .Select(h => new
                {
                    id = h.Id,
                    ticker = h.Ticker,
                    quantidade = h.Quantidade,
                    precoVenda = h.PrecoVenda,
                    valorTotal = h.ValorTotal,
                    lucro = h.Lucro,
                    data = h.Data
                })
                .ToListAsync();

            return Ok(historico);
        }

        [HttpGet("{id}/custodia")]
        public async Task<IActionResult> ObterCustodia(long id) {
            
            var cliente = await context.Clientes
                .Include(c => c.ContaGrafica)
                    .ThenInclude(cg => cg!.Custodias)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) {
                return NotFound(new { erro = "Cliente não encontrado.", codigo = "CLIENTE_NAO_ENCONTRADO" });
            }

            if (cliente.ContaGrafica == null || !cliente.ContaGrafica.Custodias.Any()) {
                
                return Ok(new { 
                    contaFilhote = cliente.ContaGrafica?.NumeroConta,
                    mensagem = "O cliente ainda não possui ativos em custódia.", 
                    posicao = new List<object>() 
                });
            }

            var posicao = cliente.ContaGrafica.Custodias.Select(c => new {
                ticker = c.Ticker,
                quantidade = c.Quantidade,
                precoMedio = c.PrecoMedio,
                valorTotalInvestido = c.Quantidade * c.PrecoMedio,
                ultimaCompra = c.DataUltimaAtualizacao
            });

            return Ok(new {
                clienteId = cliente.Id,
                nome = cliente.Nome,
                contaFilhote = cliente.ContaGrafica.NumeroConta,
                totalGeralInvestido = posicao.Sum(p => p.valorTotalInvestido),
                posicao = posicao
            });
        }

        [HttpGet("{clienteId}/carteira")]
        public async Task<IActionResult> ConsultarCarteira(long clienteId)
        {
           
            var cliente = await context.Clientes
                .Include(c => c.ContaGrafica)
                    .ThenInclude(cg => cg!.Custodias)
                .FirstOrDefaultAsync(c => c.Id == clienteId);

            if (cliente == null)
                return NotFound(new { erro = "Cliente não encontrado.", codigo = "CLIENTE_NAO_ENCONTRADO" });

            var custodias = cliente.ContaGrafica?.Custodias ?? new List<Custodia>();

            var tickersDoCliente = custodias.Select(c => c.Ticker).ToList();

            var caminhoArquivo = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "cotacoes", "COTAHIST_D27022026.TXT"));
            var cotacoesNoArquivo = parser.ExtrairCotacoes(caminhoArquivo, tickersDoCliente);

            decimal valorTotalInvestido = 0;
            decimal valorAtualCarteira = 0;
            var listaAtivosResponse = new List<object>();

            foreach (var item in custodias)
            {
                var infoB3 = cotacoesNoArquivo.FirstOrDefault(c => c.Ticker == item.Ticker);
                decimal precoAtualMercado = infoB3?.PrecoFechamento ?? item.PrecoMedio;

                decimal valorInvestidoNoAtivo = item.Quantidade * item.PrecoMedio;
                decimal valorAtualNoAtivo = item.Quantidade * precoAtualMercado;

                decimal plAtivo = (precoAtualMercado - item.PrecoMedio) * item.Quantidade;

                decimal plPercentual = item.PrecoMedio > 0 ? (plAtivo / valorInvestidoNoAtivo) * 100 : 0;

                valorTotalInvestido += valorInvestidoNoAtivo;
                valorAtualCarteira += valorAtualNoAtivo;

                listaAtivosResponse.Add(new
                {
                    ticker = item.Ticker,
                    quantidade = item.Quantidade,
                    precoMedio = Math.Round(item.PrecoMedio, 2),
                    cotacaoAtual = Math.Round(precoAtualMercado, 2),
                    valorAtual = Math.Round(valorAtualNoAtivo, 2),
                    plValor = Math.Round(plAtivo, 2),
                    plPercentual = Math.Round(plPercentual, 2)
                });
            }

            // 4. Cálculo de % e Rentabilidade
            decimal plTotal = valorAtualCarteira - valorTotalInvestido;
            decimal rentabilidadeTotal = valorTotalInvestido > 0 ? (plTotal / valorTotalInvestido) * 100 : 0;

            var ativosFinal = listaAtivosResponse.Select(a => {
                var obj = (dynamic)a;
                decimal share = valorAtualCarteira > 0 ? (obj.valorAtual / valorAtualCarteira) * 100 : 0;
                return new {
                    obj.ticker, obj.quantidade, obj.precoMedio, obj.cotacaoAtual, 
                    obj.valorAtual, obj.plValor, obj.plPercentual,
                    composicaoCarteira = Math.Round(share, 2)
                };
            });

            return Ok(new
            {
                clienteId = cliente.Id,
                nome = cliente.Nome,
                contaGrafica = cliente.ContaGrafica?.NumeroConta,
                dataConsulta = DateTime.UtcNow,
                resumo = new
                {
                    valorTotalInvestido = Math.Round(valorTotalInvestido, 2),
                    valorAtualCarteira = Math.Round(valorAtualCarteira, 2),
                    plTotal = Math.Round(plTotal, 2),
                    rentabilidadePercentual = Math.Round(rentabilidadeTotal, 2),
                    
                    ativos = ativosFinal 
                }
            });
        }    
    }

}