using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompraProgramada.Api.Data;
using CompraProgramada.Api.Models;

namespace CompraProgramada.Api.Controllers {
    
    [ApiController]
    [Route("api/usuarios")] 
    public class UsuariosController : ControllerBase {
        
        private readonly AppDbContext context;

        public UsuariosController(AppDbContext contextInfo) {
            context = contextInfo;
        }

        public class LoginRequest {
            required public string Email { get; set; }
            required public string Senha { get; set; }
        }

        public class RequestAderir {
            required public string Nome { get; set; }
            required public string Cpf { get; set; }
            required public string Email { get; set; }
            required public decimal ValorMensal { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) {
            
            if (request.Email == "admin@itau.com.br" && request.Senha == "itau123") {
                return Ok(new { role = "ADMIN", nome = "Gestor Master" });
            }

            var cliente = await context.Clientes
                .FirstOrDefaultAsync(c => c.Email == request.Email && c.Cpf == request.Senha);

            if (cliente != null) {
                return Ok(new { 
                    role = "CLIENTE", 
                    clienteId = cliente.Id, 
                    nome = cliente.Nome 
                });
            }

            return BadRequest(new { erro = "E-mail ou senha inválidos.", codigo = "LOGIN_INVALIDO" });
        }


        [HttpPost("adesao")]
        public async Task<IActionResult> Aderir([FromBody] RequestAderir request) {
            
            if (await context.Clientes.AnyAsync(c => c.Cpf == request.Cpf)) {
                return BadRequest(new { erro = "CPF ja cadastrado no sistema.", codigo = "CLIENTE_CPF_DUPLICADO" });
            }

            if (await context.Clientes.AnyAsync(c => c.Email == request.Email)) {
                return BadRequest(new { erro = "E-mail ja cadastrado no sistema.", codigo = "CLIENTE_EMAIL_DUPLICADO" });
            }

            if (request.ValorMensal < 100) {
                return BadRequest(new { erro = "O valor mensal minimo e de R$ 100,00.", codigo = "VALOR_MENSAL_INVALIDO" });
            }

            var cliente = new Cliente {
                Nome = request.Nome,
                Cpf = request.Cpf,
                Email = request.Email,
                ValorMensal = request.ValorMensal,
                DataAdesao = DateTime.UtcNow
            };

            var numeroAleatorio = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            var contaGrafica = new ContaGrafica {
                NumeroConta = $"FLH-{numeroAleatorio}",
                Tipo = "FILHOTE",
                DataCriacao = DateTime.UtcNow,
                Cliente = cliente 
            };

            context.Clientes.Add(cliente);
            context.ContasGraficas.Add(contaGrafica);
            await context.SaveChangesAsync();

            return Created($"/api/clientes/{cliente.Id}", new {
                clienteId = cliente.Id,
                nome = cliente.Nome,
                mensagem = "Conta criada com sucesso!"
            });
        }
    }
}