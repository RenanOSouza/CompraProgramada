using CompraProgramada.Api.Data;
using CompraProgramada.Api.Models;
using CompraProgramada.Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CompraProgramada.Tests
{
    public class MotorCompraServiceTests
    {
        [Fact]
        public async Task ExecutarMotor_DeveDividirValorMensalPorTres_E_ComprarAcoesCorretamente()
        {
            // Define o nome do banco em memória para ser único por teste
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            
            using (var arrangeContext = new AppDbContext(options))
            {
                var cliente = new Cliente { Nome = "Teste", Cpf = "123.456.789-00", Email = "teste@teste.com", ValorMensal = 3000, Ativo = true, DataAdesao = DateTime.UtcNow };
                var conta = new ContaGrafica { NumeroConta = "FLH-123", Tipo = "FILHOTE", Cliente = cliente };
                
                cliente.ContaGrafica = conta; 
                arrangeContext.Clientes.Add(cliente);
                arrangeContext.ContasGraficas.Add(conta);

                var cesta = new TopFive { 
                    Ativo = true, 
                    Itens = new List<ItemCesta> { 
                        new ItemCesta { Ticker = "PETR4", PercentualPeso = 20 },
                        new ItemCesta { Ticker = "VALE3", PercentualPeso = 20 },
                        new ItemCesta { Ticker = "ITUB4", PercentualPeso = 20 },
                        new ItemCesta { Ticker = "WEGE3", PercentualPeso = 20 },
                        new ItemCesta { Ticker = "BBDC4", PercentualPeso = 20 }
                    } 
                };
                arrangeContext.TopFive.Add(cesta);
                await arrangeContext.SaveChangesAsync();
            }

            // Mocks
            var mockParser = new Mock<IParserServ>();
            mockParser.Setup(p => p.ExtrairCotacoes(It.IsAny<string>(), It.IsAny<List<string>>()))
                      .Returns(new List<ResultB3> { 
                          new ResultB3 { Ticker = "PETR4", PrecoFechamento = 40.00m },
                          new ResultB3 { Ticker = "VALE3", PrecoFechamento = 60.00m },
                          new ResultB3 { Ticker = "ITUB4", PrecoFechamento = 30.00m },
                          new ResultB3 { Ticker = "WEGE3", PrecoFechamento = 35.00m },
                          new ResultB3 { Ticker = "BBDC4", PrecoFechamento = 15.00m }
                      });

            var mockKafka = new Mock<IKafkaServ>();

            
            using (var actContext = new AppDbContext(options))
            {
                var motor = new MotorServ(actContext, mockParser.Object, mockKafka.Object);
                var resultado = await motor.ExecutarMotorAsync("caminho_falso.txt");
                
                Assert.Contains("sucesso", resultado); 
            }

            
            using (var assertContext = new AppDbContext(options))
            {
                var custodiaPetr4 = await assertContext.Custodias.FirstOrDefaultAsync(c => c.Ticker == "PETR4");
                
                Assert.NotNull(custodiaPetr4); 
                Assert.Equal(5, custodiaPetr4.Quantidade); 
                Assert.Equal(40.00m, custodiaPetr4.PrecoMedio); 
            }
        }
    }
}